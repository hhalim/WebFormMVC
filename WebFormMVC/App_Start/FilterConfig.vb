Imports System.Web
Imports System.Web.Mvc
Imports System.Configuration
Imports System.IO
Imports System.Text
Imports System.Web.Security
Imports Newtonsoft.Json

Public Class FilterConfig

    Public Shared Sub RegisterGlobalFilters(filters As GlobalFilterCollection)
        filters.Add(New HandleErrorAttribute())
        filters.Add(New JsonNetActionFilter()) 'Allows returning ISO date format, instead of MS date format
    End Sub

End Class

Public Class JsonNetActionFilter
    Inherits ActionFilterAttribute
    Public Overrides Sub OnActionExecuted(filterContext As ActionExecutedContext)
        If filterContext.Result.GetType() = GetType(JsonResult) Then
            ' Get the standard result object with unserialized data
            Dim result As JsonResult = TryCast(filterContext.Result, JsonResult)

            ' Replace it with our new result object and transfer settings
            ' Later on when ExecuteResult will be called it will be the
            ' function in JsonNetResult instead of in JsonResult
            filterContext.Result = New JsonNetResult() With {
                    .ContentEncoding = result.ContentEncoding,
                    .ContentType = result.ContentType,
                    .Data = result.Data,
                    .JsonRequestBehavior = result.JsonRequestBehavior
                }
        End If
        MyBase.OnActionExecuted(filterContext)
    End Sub
End Class

Public Class JsonNetResult
    Inherits JsonResult

    Public Sub New()
        Dim encryptionKey As String = ConfigurationManager.AppSettings("EncryptionKey")
        Settings = New JsonSerializerSettings() With {
                .ReferenceLoopHandling = ReferenceLoopHandling.Error,
                .DateFormatHandling = DateFormatHandling.IsoDateFormat,
                .ContractResolver = New EncryptedStringPropertyResolver(encryptionKey) 'MVC to use camelCase and JsonEncrypt attribute
            }
    End Sub

    Private m_settings As JsonSerializerSettings
    Public Property Settings() As JsonSerializerSettings
        Get
            Return m_settings
        End Get
        Private Set
            m_settings = Value
        End Set
    End Property

    Public Overrides Sub ExecuteResult(context As ControllerContext)
        If context Is Nothing Then
            Throw New ArgumentNullException("context")
        End If

        'Prevents possible Json Hijack http://haacked.com/archive/2009/06/25/json-hijacking.aspx/
        If JsonRequestBehavior = JsonRequestBehavior.DenyGet AndAlso String.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("Json GET is not allowed. To allow GET requests, set JsonRequestBehavior to AllowGet, such as: return Json(data, JsonRequestBehavior.AllowGet).")
        End If

        Dim response As HttpResponseBase = context.HttpContext.Response
        response.ContentType = If(String.IsNullOrWhiteSpace(ContentType), "application/json", ContentType)

        If ContentEncoding IsNot Nothing Then
            response.ContentEncoding = ContentEncoding
        End If
        If Data Is Nothing Then
            Return
        End If

        Dim scriptSerializer As JsonSerializer = JsonSerializer.Create(Settings)

        Using sw As New StringWriter()
            scriptSerializer.Serialize(sw, Data)
            response.Write(sw.ToString())
        End Using
    End Sub

End Class

