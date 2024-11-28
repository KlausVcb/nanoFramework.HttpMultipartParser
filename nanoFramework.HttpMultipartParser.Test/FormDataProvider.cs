using System.Diagnostics;
using System.IO;
using System.Text;

namespace nanoFramework.HttpMultipartParser.Test
{
    internal static class FormDataProvider
    {
        public static Stream CreateFormWithParameters()
        {
            var content = @"------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""param1""

value1
------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""param2""

value2
------WebKitFormBoundarySZFRSm4A2LAZPpUu--";

            return new MemoryStream(Encoding.UTF8.GetBytes(content)) { Position = 0 };
        }

        public static Stream CreateFormWithFile()
        {
            var content = @"------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""file""; filename=""somefile.json""
Content-Type: application/json

{
  ""Name"": ""Chuck Norris"",
  ""Age"": 999
}
------WebKitFormBoundarySZFRSm4A2LAZPpUu--";

            return new MemoryStream(Encoding.UTF8.GetBytes(content)) { Position = 0 };
        }

        public static Stream CreateFormWithFiles()
        {
            var content = @"------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""file""; filename=""first.json""
Content-Type: application/json

{
  ""Name"": ""Chuck Norris"",
  ""Age"": 999
}
------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""file""; filename=""second.json""
Content-Type: application/json

{
  ""Name"": ""Darth Vader"",
  ""Age"": 9999
}
------WebKitFormBoundarySZFRSm4A2LAZPpUu--";

            return new MemoryStream(Encoding.UTF8.GetBytes(content)) { Position = 0 };
        }

        public static Stream CreateFormWithEverything()
        {
            var content = @"------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""param1""

value1
------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""param2""

value2
------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""file""; filename=""first.json""
Content-Type: application/json

{
  ""Name"": ""Chuck Norris"",
  ""Age"": 999
}
------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""file""; filename=""second.json""
Content-Type: application/json

{
  ""Name"": ""Darth Vader"",
  ""Age"": 9999
}
------WebKitFormBoundarySZFRSm4A2LAZPpUu--";

            return new MemoryStream(Encoding.UTF8.GetBytes(content)) { Position = 0 };
        }

        public static string CreateContent(int size)
        {
            var sb = new StringBuilder(size);

            while (sb.Length < size)
                sb.Append("HMLTncevuycfsoiS7cAHhiJq8CI2pTnHhJJb3MfwRB9qlK0VryH8AuJAQzhguP1Z");

            return sb.ToString();
        }

        public static Stream CreateFormWithFile(string file)
        {
            var content = @$"------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; name=""file""; filename=""somefile.json""
Content-Type: application/json

{file}
------WebKitFormBoundarySZFRSm4A2LAZPpUu--";

            return new MemoryStream(Encoding.UTF8.GetBytes(content)) { Position = 0 };
        }

        public static Stream CreateEmptyForm()
        {
            var content = @"------WebKitFormBoundarySZFRSm4A2LAZPpUu



------WebKitFormBoundarySZFRSm4A2LAZPpUu--";

            return new MemoryStream(Encoding.UTF8.GetBytes(content)) { Position = 0 };
        }

        public static Stream CreateInvalidForm()
        {
            //missing the name parameter should fail
            var content = @"------WebKitFormBoundarySZFRSm4A2LAZPpUu
Content-Disposition: form-data; invalid=""blah""

value1
------WebKitFormBoundarySZFRSm4A2LAZPpUu--";

            return new MemoryStream(Encoding.UTF8.GetBytes(content)) { Position = 0 };
        }
    }
}
