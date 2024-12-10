using MH_TicketingSystem.Models;
using MH_TicketingSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace MH_TicketingSystem.Controllers
{

	/// <summary>
	/// Use in managing the priority level of the ticket that is use in creating the ticket
	/// </summary>
	public class PriorityLevelController : Controller
	{
		private readonly ApplicationDbContext _context;

		public PriorityLevelController(ApplicationDbContext context)
		{
			_context = context;
		}


		public IActionResult Index()
		{
			return View();
		}

		/// <summary>
		///  Get all priority levels
		/// </summary>
		/// <returns> JSON containing the success flag and the priority level data</returns>
		[HttpGet]
		public JsonResult GetAllPriorityLevels()
		{
			var pLevels =  _context.PriorityLevels.ToList();
			return Json(new { success = true, pLevels });
		}


		/// <summary>
		/// Creating new priority level
		/// </summary>
		/// <param name="priorityLevel"></param>
		/// <returns>Json Object</returns>
		[HttpPost]
		public async Task<JsonResult> Create(PriorityLevel priorityLevel)
		{
			if (!ModelState.IsValid)
			{
				return Json(new { success = true, message = "Model validation failed." });
			}

			// Check if there is duplicate.
			var isDuplicate = _context.PriorityLevels
				.Any(p =>
					(p.PriorityLevelName == priorityLevel.PriorityLevelName ||
					p.PriorityLevelColor == priorityLevel.PriorityLevelColor) &&
					p.IsPriorityLevelActive
			);

			if (isDuplicate)
			{
				return Json(new { success = false, message = "Duplicate priority level." });
			}
			else
			{
                try
                {
                    await _context.AddAsync(priorityLevel);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Priority level details saved." });

                }
                catch (Exception ex)
                {
                    return Json(new { success = true, message = $"Error saving priority level: {ex.Message}" });
                }
            }
		}


		/// <summary>
		/// Use in retrieving data to edit
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet]
		public async Task<JsonResult> Edit(int id)
		{
			var pLevel = await _context.PriorityLevels.FindAsync(id);
			if (pLevel == null)
			{
				return Json(new { success = false, message = $"Priority Level not found with id {id}" });
			}
			return Json(new { success = true, pLevel });
		}

		/// <summary>
		/// Update the priority level
		/// </summary>
		/// <param name="priorityLevel"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult Update(PriorityLevel priorityLevel)
		{
			if (!ModelState.IsValid)
			{
				return Json(new { success = true, message = "Model validation error." });
			}

			try
			{
				_context.PriorityLevels.Update(priorityLevel);
				_context.SaveChanges();
				return Json(new { success = true, message = "Priority Level details updated." });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = $"Error updating priority level: {ex.Message}" });
			}
		}


        /// <summary>
        /// Update the priority level
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
		public JsonResult Delete(int id)
		{
			var pLevel = _context.PriorityLevels.Find(id);
			if(pLevel == null)
			{
                return Json(new { success = false, message = $"Priority level not found with id {id}" });
            }

			try
			{
				// Check if there is a record in ticket table
				var hasRecord = false;
				if (!hasRecord)
				{
					_context.PriorityLevels.Remove(pLevel);
					_context.SaveChanges();
                    return Json(new { success = true, message = "Priority level deleted successfully." });
                }
                return Json(new { success = false, message = "This priority level is referenced in other data and cannot be removed." });
            }
            catch (Exception ex)
			{
				return Json(new { success = false, message = $"Error deleting the level: {ex.Message}" });
            }
		}

	}
}
