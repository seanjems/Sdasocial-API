using sdakccapi.Dtos;

namespace sdakccapi.StaticDetails
{
    public class OtherMethods
    {

        private readonly IWebHostEnvironment _webHostEnvironment;

        public OtherMethods(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        //Save imageupload file
        public async Task<ResultObject> SaveFile(IFormFile dto)
        {
            //handle image upload
            string webRootPath = _webHostEnvironment.WebRootPath;
            string link = null;
            var obj = new ResultObject();

            var files = dto;
            try
            {
                if (files.Length > 0)
                {
                    string uploadPath = Path.Combine(webRootPath, @"\media\images".TrimStart('\\')); // doesnt work if second path has a trailling slash
                    string extension = Path.GetExtension(files.FileName);
                    if (!(extension.ToLower() == ".jpg" || extension.ToLower() == ".png" || extension.ToLower() == ".jpeg"))
                    {
                        throw new ApplicationException("The image file type must be jpg or png");

                    }

                    string fileNewName = Guid.NewGuid().ToString() + extension;

                    using (var fileStream = new FileStream(Path.Combine(uploadPath, fileNewName), FileMode.Create))
                    {
                        await files.CopyToAsync(fileStream);
                    }
                    link = @"media/images/" + fileNewName;
                }

                string msg = "Upload of Image Successful";
                obj.ReturnCode = "200";
                obj.ReturnDescription = msg;
                obj.Response = "Success";
                obj.Message = msg;
                obj.Link = link;

            }
            catch (Exception ex)
            {
                string msg = "An Error has occurred while attempting to Upload Image: Inner Exception: " + ex.Message;
                obj.ReturnCode = "501";
                obj.ReturnDescription = msg;
                obj.Response = "Failed";
                obj.Message = msg;
            }
            return obj;
        }

    }
}
