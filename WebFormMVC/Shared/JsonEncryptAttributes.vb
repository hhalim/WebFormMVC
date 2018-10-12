Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Security.Cryptography
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization

<AttributeUsage(AttributeTargets.[Property])>
Public Class JsonEncryptAttribute
    Inherits Attribute
End Class

Public Class EncryptedStringPropertyResolver
    Inherits CamelCasePropertyNamesContractResolver

    Private encryptionKeyBytes As Byte()

    Public Sub New(ByVal encryptionKey As String)
        If encryptionKey Is Nothing Then Throw New ArgumentNullException("encryptionKey")

        Using sha As SHA256Managed = New SHA256Managed()
            Me.encryptionKeyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey))
        End Using
    End Sub

    Protected Overrides Function CreateProperties(ByVal type As Type, ByVal memberSerialization As MemberSerialization) As IList(Of JsonProperty)
        Dim props As IList(Of JsonProperty) = MyBase.CreateProperties(type, memberSerialization)

        For Each prop As JsonProperty In props.Where(Function(p) p.PropertyType = GetType(String))
            Dim pi As PropertyInfo = type.GetProperty(prop.UnderlyingName)

            If pi IsNot Nothing AndAlso pi.GetCustomAttribute(GetType(JsonEncryptAttribute), True) IsNot Nothing Then
                prop.ValueProvider = New EncryptedValueProvider(pi, encryptionKeyBytes)
            End If
        Next

        Return props
    End Function

    Class EncryptedValueProvider
        Implements IValueProvider

        Private targetProperty As PropertyInfo
        Private encryptionKey As Byte()

        Public Sub New(ByVal targetProperty As PropertyInfo, ByVal encryptionKey As Byte())
            Me.targetProperty = targetProperty
            Me.encryptionKey = encryptionKey
        End Sub

        Public Function GetValue(ByVal target As Object) As Object Implements IValueProvider.GetValue
            Dim value = Convert.ToString(targetProperty.GetValue(target))
            Dim buffer As Byte() = Encoding.UTF8.GetBytes(value)

            Using inputStream As MemoryStream = New MemoryStream(buffer, False)

                Using outputStream As MemoryStream = New MemoryStream()

                    Using aes As AesManaged = New AesManaged With {
                        .Key = encryptionKey
                    }
                        Dim iv As Byte() = aes.IV
                        outputStream.Write(iv, 0, iv.Length)
                        outputStream.Flush()
                        Dim encryptor As ICryptoTransform = aes.CreateEncryptor(encryptionKey, iv)

                        Using cryptoStream As CryptoStream = New CryptoStream(outputStream, encryptor, CryptoStreamMode.Write)
                            inputStream.CopyTo(cryptoStream)
                        End Using

                        Return Convert.ToBase64String(outputStream.ToArray())
                    End Using
                End Using
            End Using
        End Function

        Public Sub SetValue(ByVal target As Object, ByVal value As Object) Implements IValueProvider.SetValue
            Dim buffer As Byte() = Convert.FromBase64String(CStr(value))

            Using inputStream As MemoryStream = New MemoryStream(buffer, False)

                Using outputStream As MemoryStream = New MemoryStream()

                    Using aes As AesManaged = New AesManaged With {
                        .Key = encryptionKey
                    }
                        Dim iv As Byte() = New Byte(15) {}
                        Dim bytesRead As Integer = inputStream.Read(iv, 0, 16)

                        If bytesRead < 16 Then
                            Throw New CryptographicException("IV is missing or invalid.")
                        End If

                        Dim decryptor As ICryptoTransform = aes.CreateDecryptor(encryptionKey, iv)

                        Using cryptoStream As CryptoStream = New CryptoStream(inputStream, decryptor, CryptoStreamMode.Read)
                            cryptoStream.CopyTo(outputStream)
                        End Using

                        Dim decryptedValue As String = Encoding.UTF8.GetString(outputStream.ToArray())
                        targetProperty.SetValue(target, decryptedValue)
                    End Using
                End Using
            End Using
        End Sub
    End Class
End Class
