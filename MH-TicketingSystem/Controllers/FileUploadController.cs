using Microsoft.AspNetCore.Mvc;

namespace MH_TicketingSystem.Controllers
{

	/// <summary>
	/// This controller only use in uploading file
	/// </summary>
	public class FileUploadController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		/// <summary>
		/// Use in uploading the file in this project folder
		/// and saving the file path and name in database
		/// </summary>
		/// <returns></returns>
		[HttpPost("FileUpload")]
		public async Task<IActionResult> FileUpload()
		{
			if (Request.HasFormContentType && Request.Form.Files.Count > 0)
			{
				var file = Request.Form.Files[0];
				string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
				string uploadFolder = Path.Combine(wwwRootPath, "uploads", "conversations");

				// Ensure the directory exists
				//Directory.CreateDirectory(uploadFolder); it is already existed

				string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
				string filePath = Path.Combine(uploadFolder, uniqueFileName);

				await using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				// Return the file path for use in the application
				string relativePath = Path.Combine("uploads", "conversations", uniqueFileName);
				return Ok(new
				{
					FileName = file.FileName,
					FilePath = filePath
				});
			}

			return BadRequest("No file was uploaded.");
		}
	}
}
