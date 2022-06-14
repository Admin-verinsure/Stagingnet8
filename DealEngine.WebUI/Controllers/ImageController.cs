using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DealEngine.WebUI.Models.Image;
using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces;
using DealEngine.Infrastructure.FluentNHibernate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.Drawing;
using Microsoft.Extensions.Logging;
namespace DealEngine.WebUI.Controllers
{
    [Authorize]
    public class ImageController : BaseController
    {
        IMapperSession<CKImage> _ckimageRepository;
        IMapperSession<Document> _fileRepository;
        private readonly ICKImageService _ckimageService;
        private readonly IProductService _iproductService;
        private readonly IWebHostEnvironment _hostingEnv;
        //IMapperSession<User> _userRepository;
        IApplicationLoggingService _applicationLoggingService;
        IAppSettingService _appSettingService;
        ILogger<ImageController> _logger;

        public ImageController(IUserService userRepository, 
            IMapperSession<CKImage> ckimageRepository, 
            IMapperSession<Document> fileRepository, 
            ICKImageService ckimageService,
            IProductService iproductService, 
            IWebHostEnvironment hostingEnv,
            IAppSettingService appSettingService,
            IApplicationLoggingService applicationLoggingService,
            ILogger<ImageController> logger
            )
            : base(userRepository)
        {
            _applicationLoggingService=applicationLoggingService;
            _logger=logger;
            _ckimageRepository = ckimageRepository;
            _fileRepository = fileRepository;
            _ckimageService = ckimageService;
            _iproductService = iproductService;
            _hostingEnv = hostingEnv;
            _appSettingService = appSettingService;
        }
        [HttpGet]
        public async Task<ViewResult> ManageImage()
        {
            var list = await _ckimageService.GetAllImages();
            var list2 = await _iproductService.GetAllProducts();
            ImageViewModel model = new ImageViewModel();
            model.Item = list;
            model.Products = list2;

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllImages()
        {
            var user = await CurrentUser();
            if (user.IsLoggedout)
                return PageNotFound();

            if (user == null)
                return PageNotFound();

            var model = await _ckimageService.GetAllImages();
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> CKGetAllImages()
        {
            var user = await CurrentUser();
            if (user.IsLoggedout)
                return PageNotFound();

            if (user == null)
                return PageNotFound();

            var model = await _ckimageService.GetAllImages();
            return Json(model);
        }
        [HttpPost]
        public async Task<IActionResult> Upload(ImageViewModel model)
        {
            string CKImageFolderPath = _appSettingService.CKImagePath;

            var user = await CurrentUser();
            if (model != null)
            {                
                if (model.Image != null)
                { 
                    var contentType = model.Image.ContentType;

                    #region Extension related
                    var extension = "";                        
                    if (contentType == "image/jpeg")
                    {
                        extension = ".jpg";
                    }
                    else if (contentType == "image/png")
                    {
                        extension = ".png";
                    }
                    else
                    {
                        throw new FileFormatException("Invalid File Type");
                    }
                    #endregion

                    if (model.Name == null || model.Name.Length < 1)
                    {
                        model.Name = model.Image.FileName;
                        extension = "";
                    }

                    var thumbnailFilename = "_" + model.Name + extension;
                    var filename = model.Name + extension;
                    var imageThumbnailPath = Path.Combine(CKImageFolderPath, thumbnailFilename);
                    var imagePath = Path.Combine(CKImageFolderPath, filename);

                    // Save Thumbnail Locally
                    try
                    {
                        Stream stream = model.Image.OpenReadStream();
                        System.Drawing.Image thumbnail = GetReducedImage(100,100,stream);

                        if (thumbnail != null)
                        {
                            thumbnail.Save(imageThumbnailPath, thumbnail.RawFormat);
                        }
                        else
                        {
                            throw new ArgumentNullException("Image is null.");
                        }
                    }

                    catch(Exception ex)
                    {
                        await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                    }

                    // Save Image Locally
                    try
                    { 
                        using (var fileStream = new FileStream(imagePath, FileMode.Create))
                        {
                            await model.Image.CopyToAsync(fileStream);
                        }

                        CKImage newCKImage = new CKImage
                        {
                            Name = filename,
                            Path = filename,
                            ThumbPath = thumbnailFilename
                        };

                        CKImage dupe = await _ckimageService.GetCKImage(model.Image.FileName);
                        if (dupe != null && dupe.Path == newCKImage.Path)
                        {
                            // DO NOTHING (its a duplicate)
                        }
                        else
                        {
                            await _ckimageRepository.AddAsync(newCKImage);
                        }
                    }
                    catch (Exception Ex)
                    {
                        Console.WriteLine(Ex.ToString());
                    }
                }
                return Redirect("~/Image/ManageImage");          
            }
            return Redirect("~/Image/ManageImage");
        }
        [HttpPost]
        public async Task<IActionResult> CKUpload()
        { 
            IFormFileCollection files = HttpContext.Request.Form.Files;
            int fileCount = files.Count;
            IFormFile file = null;

            if (fileCount == 0)
            {
                return BadRequest();
            }
            else if (fileCount == 1)
            {
                foreach (var x in files)
                {
                    file = x;
                }
            }
            else
            {
                return BadRequest();
            }

            if (file != null && file.Length > 0)
            {
                string CKImageFolderPath = _appSettingService.CKImagePath;

                var thumbnailFilename = "_" + file.FileName;
                var filename = file.FileName;

                var imageThumbnailPath = Path.Combine(CKImageFolderPath, thumbnailFilename);
                var imagePath = Path.Combine(CKImageFolderPath, filename);

                var url = "https://" + _appSettingService.domainQueryString + "/Image/" + filename;
                var json = Json(new { url });

                try
                {
                    // Create Thumbnail and save it
                    Stream stream = file.OpenReadStream();
                    System.Drawing.Image thumbnail = GetReducedImage(100, 100, stream);
                    if (thumbnail != null)
                    {
                        thumbnail.Save(imageThumbnailPath, thumbnail.RawFormat);
                    }
                    else
                    {
                        throw new ArgumentNullException("Image is null.");
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                try
                {
                    using (var fileStream = new FileStream(imagePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    CKImage newCKImage = new CKImage
                    {
                        Name = filename,
                        Path = filename,
                        ThumbPath = thumbnailFilename
                    };

                    CKImage dupe = await _ckimageService.GetCKImage(filename);
                    if (dupe != null && dupe.Path == newCKImage.Path)
                    {
                        // DO NOTHING (its a duplicate)
                    }
                    else
                    {
                        await _ckimageRepository.AddAsync(newCKImage);
                    }
                    return json;
                }

                catch (Exception Ex)
                {
                    Console.WriteLine(Ex.ToString());
                }
            }
            else
            {
                return BadRequest();
            }

            return BadRequest();
        }
        [HttpPost]
        public async Task<IActionResult> UploadDoc(ImageViewModel model)
        {

            var user = await CurrentUser();
            if (model != null)
            {
                if (model.File != null)
                {

                    var contentType = model.File.ContentType;
                    var extension = "";
                    var filename = "";
                    
                    if (contentType == "application/pdf")
                    {
                        extension = ".pdf";
                    }                    
                    else
                    {
                        throw new FileFormatException("Invalid File Type");
                    }

                    if (model.Name != null){
                        filename = model.Name + extension;
                    }
                    else {
                        filename = model.File.FileName;
                    }

                    var path = Path.Combine(_hostingEnv.WebRootPath, "files", model.Product, "attachmentfiles");
                    System.IO.Directory.CreateDirectory(path);
                    path = Path.Combine(path, filename);
                    
                    try
                    {
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await model.File.CopyToAsync(fileStream);
                        }
                        
                        DealEngine.Domain.Entities.Document newFile = new DealEngine.Domain.Entities.Document {
                            Name = filename,
                            Description = "Local PDF for " + model.Product,
                            DocumentType = 0,
                            IsTemplate = true,
                            ContentType = model.File.ContentType,
                            FileRendered = false,                                                  
                            Path = path
                        };

                        await _fileRepository.AddAsync(newFile);

                        Guid productID = Guid.Parse(model.Product);
                        Product myProduct = await _iproductService.GetProductById(productID);                        
                        myProduct.Documents.Add(newFile);
                        await _iproductService.UpdateProduct(myProduct);
                    }

                    catch (Exception Ex)
                    {
                        Console.WriteLine(Ex.ToString());
                    }

                }
                return Redirect("~/Image/ManageImage");
            }
            return Redirect("~/Image/ManageImage");
        }
        [HttpPost]
       public IActionResult Delete()
       {      
            return View("~/Views/Image/ManageImage.cshtml");
       }

        public System.Drawing.Image GetReducedImage(int Width, int Height, Stream resourceImage)
        {
            try
            {
                System.Drawing.Image fullImage = System.Drawing.Image.FromStream(resourceImage);
                System.Drawing.Image thumbnail = fullImage.GetThumbnailImage(Width, Height, ()=> false, IntPtr.Zero);

                return thumbnail;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }

}