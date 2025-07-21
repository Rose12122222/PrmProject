using AdminHereWeGo.Models;
using AdminHereWeGo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminHereWeGo.Pages
{
    public class CreateModel : PageModel
    {
        private readonly FirebaseService _firebaseService;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        public ItemModel Item { get; set; }

        public Dictionary<string, string> CategoryTitles { get; set; }

        public CreateModel(FirebaseService firebaseService, ILogger<CreateModel> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            _logger.LogInformation("Starting OnGetAsync for Create page.");
            Item = new ItemModel
            {
                PicUrl = new List<string>(),
                Size = new List<string>()
            };
            CategoryTitles = await _firebaseService.GetCategoryTitlesAsync();
            _logger.LogInformation($"Loaded {CategoryTitles.Count} categories for Create page.");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Starting OnPostAsync for Create page.");

            ModelState.Remove("Item.Id");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Errors: {Errors}", string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                CategoryTitles = await _firebaseService.GetCategoryTitlesAsync();
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Item.Title))
            {
                ModelState.AddModelError("Item.Title", "Tên món ăn là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(Item.Description))
            {
                ModelState.AddModelError("Item.Description", "Mô tả là bắt buộc.");
            }
            if (Item.Price <= 0)
            {
                ModelState.AddModelError("Item.Price", "Giá phải lớn hơn 0.");
            }
            if (Item.Rating < 0 || Item.Rating > 5)
            {
                ModelState.AddModelError("Item.Rating", "Đánh giá phải từ 0 đến 5.");
            }
            if (string.IsNullOrWhiteSpace(Item.CategoryId))
            {
                ModelState.AddModelError("Item.CategoryId", "Danh mục là bắt buộc.");
            }
            if (string.IsNullOrWhiteSpace(Item.SellerName))
            {
                ModelState.AddModelError("Item.SellerName", "Tên người bán là bắt buộc.");
            }
            if (string.IsNullOrEmpty(Item.SellerPic))
            {
                ModelState.AddModelError("Item.SellerPic", "URL hình ảnh người bán là bắt buộc.");
            }
            if (!string.IsNullOrEmpty(Request.Form["Item.PicUrl"]))
            {
                Item.PicUrl = Request.Form["Item.PicUrl"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                if (!Item.PicUrl.Any())
                {
                    ModelState.AddModelError("Item.PicUrl", "Cần ít nhất một URL hình ảnh sản phẩm.");
                }
            }
            else
            {
                ModelState.AddModelError("Item.PicUrl", "URL hình ảnh sản phẩm là bắt buộc.");
            }
            if (!string.IsNullOrEmpty(Request.Form["Item.Size"]))
            {
                Item.Size = Request.Form["Item.Size"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                if (!Item.Size.Any())
                {
                    ModelState.AddModelError("Item.Size", "Cần ít nhất một kích thước.");
                }
            }
            else
            {
                ModelState.AddModelError("Item.Size", "Kích thước là bắt buộc.");
            }
            if (!string.IsNullOrEmpty(Request.Form["Item.SellerTell"]))
            {
                if (long.TryParse(Request.Form["Item.SellerTell"], out var sellerTell))
                {
                    Item.SellerTell = sellerTell;
                }
                else
                {
                    ModelState.AddModelError("Item.SellerTell", "Số điện thoại không hợp lệ. Vui lòng nhập số.");
                }
            }
            else
            {
                ModelState.AddModelError("Item.SellerTell", "Số điện thoại là bắt buộc.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation failed. Errors: {Errors}", string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                CategoryTitles = await _firebaseService.GetCategoryTitlesAsync();
                return Page();
            }

            _logger.LogInformation("Validation passed. Creating new item: {Title}", Item.Title);
            await _firebaseService.CreateItemAsync(Item);
            _logger.LogInformation("Item created successfully. Redirecting to Index.");
            return RedirectToPage("Index");
        }
    }
}