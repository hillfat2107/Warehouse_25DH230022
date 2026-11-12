using System.Linq;
using System.Web.Mvc;
using Buoi9.Models;

namespace Buoi9.Controllers
{
    public class ProductsController : Controller
    {
        private readonly Warehouse_25DH230022Entities db =
            new Warehouse_25DH230022Entities();

        public ActionResult Index()
        {
            var products = db.Products
                             .OrderBy(p => p.ProductID)
                             .ToList();
            return View(products);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            int id,
            [Bind(Include = "ProductName,Category,Price")] Product form)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            if (string.IsNullOrWhiteSpace(form.ProductName))
                ModelState.AddModelError("ProductName", "Tên sản phẩm không được rỗng.");
            else if (form.ProductName.Trim().Length > 100)
                ModelState.AddModelError("ProductName", "Tên tối đa 100 ký tự.");

            if (string.IsNullOrWhiteSpace(form.Category))
                ModelState.AddModelError("Category", "Danh mục không được rỗng.");
            else if (form.Category.Trim().Length > 100)
                ModelState.AddModelError("Category", "Danh mục tối đa 100 ký tự.");

            if (form.Price <= 0)
                ModelState.AddModelError("Price", "Đơn giá phải lớn hơn 0.");

            if (!ModelState.IsValid)
            {
                form.ProductID = product.ProductID;
                form.Quantity = product.Quantity;
                return View(form);
            }

            // Chỉ cập nhật 3 trường được phép sửa.
            product.ProductName = form.ProductName.Trim();
            product.Category = form.Category.Trim();
            product.Price = form.Price;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // Giữ menu câu 3–7 hoạt động trong lúc chưa làm các câu đó.
        [HttpGet]
        public ActionResult Search(string keyword = "", string stock = "", string sort = "asc")
        {
            keyword = (keyword ?? "").Trim();
            stock = stock ?? "";
            sort = sort ?? "asc";

            var query = db.Products.AsQueryable();

            // Tìm một phần tên, không phân biệt chữ hoa/thường.
            if (keyword.Length > 0)
            {
                string lowerKeyword = keyword.ToLower();
                query = query.Where(p =>
                    p.ProductName.ToLower().Contains(lowerKeyword));
            }

            // Lọc mức tồn kho; có thể kết hợp với tìm tên.
            if (stock == "out")
                query = query.Where(p => p.Quantity == 0);
            else if (stock == "low")
                query = query.Where(p => p.Quantity >= 1 && p.Quantity <= 4);
            else if (stock == "enough")
                query = query.Where(p => p.Quantity >= 5);

            // Sắp xếp bằng LINQ/EF.
            if (sort == "desc")
                query = query.OrderByDescending(p => p.Price)
                             .ThenBy(p => p.ProductID);
            else
                query = query.OrderBy(p => p.Price)
                             .ThenBy(p => p.ProductID);

            ViewBag.Keyword = keyword;
            ViewBag.Stock = stock;
            ViewBag.Sort = sort;

            return View(query.ToList());
        }

        [HttpGet]
        public ActionResult Inventory()
        {
            ViewBag.Products = new SelectList(
                db.Products.OrderBy(p => p.ProductName).ToList(),
                "ProductID", "ProductName");

            ViewBag.Result = TempData["InventoryResult"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Inventory(int? productId, string actualQuantity)
        {
            int newQuantity;

            if (!productId.HasValue)
                ModelState.AddModelError("productId", "Hãy chọn sản phẩm.");

            if (!int.TryParse(actualQuantity, out newQuantity) || newQuantity < 0)
                ModelState.AddModelError(
                    "actualQuantity",
                    "Số lượng thực tế phải là số nguyên không âm.");

            if (!ModelState.IsValid)
            {
                ViewBag.Products = new SelectList(
                    db.Products.OrderBy(p => p.ProductName).ToList(),
                    "ProductID", "ProductName", productId);
                return View();
            }

            var product = db.Products.Find(productId.Value);
            if (product == null)
            {
                ModelState.AddModelError("productId", "Sản phẩm không tồn tại.");
                ViewBag.Products = new SelectList(
                    db.Products.OrderBy(p => p.ProductName).ToList(),
                    "ProductID", "ProductName");
                return View();
            }

            int oldQuantity = product.Quantity;
            long delta = (long)newQuantity - oldQuantity;

            string difference = delta > 0
                ? $"Chênh lệch tăng {delta}"
                : delta < 0
                    ? $"Chênh lệch giảm {-delta}"
                    : "Không chênh lệch";

            product.Quantity = newQuantity;
            db.SaveChanges();

            TempData["InventoryResult"] =
                $"{product.ProductName}: {difference}. " +
                $"Quantity cũ = {oldQuantity}; Quantity mới = {newQuantity}.";

            return RedirectToAction("Inventory");
        }

        [HttpGet]
        public ActionResult InventoryTwo()
        {
            ViewBag.Result = TempData["InventoryTwoResult"];
            ViewBag.Error = TempData["InventoryTwoError"];

            return View(db.Products.OrderBy(p => p.ProductID).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InventoryTwo(
            int? productId1, string actualQuantity1,
            int? productId2, string actualQuantity2)
        {
            int quantity1, quantity2;

            if (!productId1.HasValue)
                ModelState.AddModelError("productId1", "ID sản phẩm 1 không hợp lệ.");

            if (!productId2.HasValue)
                ModelState.AddModelError("productId2", "ID sản phẩm 2 không hợp lệ.");

            if (productId1.HasValue && productId2.HasValue &&
                productId1.Value == productId2.Value)
                ModelState.AddModelError("productId2", "Phải chọn hai ID khác nhau.");

            if (!int.TryParse(actualQuantity1, out quantity1) || quantity1 < 0)
                ModelState.AddModelError(
                    "actualQuantity1", "Số lượng 1 phải là số nguyên không âm.");

            if (!int.TryParse(actualQuantity2, out quantity2) || quantity2 < 0)
                ModelState.AddModelError(
                    "actualQuantity2", "Số lượng 2 phải là số nguyên không âm.");

            if (!ModelState.IsValid)
                return View(db.Products.OrderBy(p => p.ProductID).ToList());

            var product1 = db.Products.Find(productId1.Value);
            var product2 = db.Products.Find(productId2.Value);

            if (product1 == null)
                ModelState.AddModelError("productId1", "ID sản phẩm 1 không tồn tại.");

            if (product2 == null)
                ModelState.AddModelError("productId2", "ID sản phẩm 2 không tồn tại.");

            if (!ModelState.IsValid)
                return View(db.Products.OrderBy(p => p.ProductID).ToList());

            int old1 = product1.Quantity;
            int old2 = product2.Quantity;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    product1.Quantity = quantity1;
                    product2.Quantity = quantity2;

                    db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    TempData["InventoryTwoError"] =
                        "Lưu thất bại. Không cập nhật số lượng của hai sản phẩm.";
                    return RedirectToAction("InventoryTwo");
                }
            }

            TempData["InventoryTwoResult"] =
                $"Đã cập nhật cùng một transaction: " +
                $"ID {product1.ProductID}: {old1} → {quantity1}; " +
                $"ID {product2.ProductID}: {old2} → {quantity2}.";

            return RedirectToAction("InventoryTwo");
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "ProductName,Category,Price,Quantity")] Product product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
                ModelState.AddModelError("ProductName", "Tên sản phẩm không được rỗng.");
            else if (product.ProductName.Trim().Length > 100)
                ModelState.AddModelError("ProductName", "Tên tối đa 100 ký tự.");

            if (string.IsNullOrWhiteSpace(product.Category))
                ModelState.AddModelError("Category", "Danh mục không được rỗng.");
            else if (product.Category.Trim().Length > 100)
                ModelState.AddModelError("Category", "Danh mục tối đa 100 ký tự.");

            if (product.Price <= 0)
                ModelState.AddModelError("Price", "Đơn giá phải lớn hơn 0.");

            if (product.Quantity < 0)
                ModelState.AddModelError("Quantity", "Số lượng phải là số nguyên không âm.");

            if (!ModelState.IsValid)
                return View(product);

            string normalizedName = product.ProductName.Trim().ToLower();

            bool duplicate = db.Products.Any(p =>
                p.ProductName.Trim().ToLower() == normalizedName);

            if (duplicate)
            {
                ModelState.AddModelError(
                    "ProductName",
                    "Tên sản phẩm đã tồn tại, kể cả khi khác chữ hoa/thường hoặc khoảng trắng.");
                return View(product);
            }

            product.ProductName = product.ProductName.Trim();
            product.Category = product.Category.Trim();

            db.Products.Add(product);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Statistics()
        {
            ViewBag.TotalProducts = db.Products.Count();

            ViewBag.OutOfStock = db.Products.Count(p => p.Quantity == 0);

            ViewBag.TotalValue = db.Products
                .Sum(p => (decimal?)(p.Price * p.Quantity)) ?? 0m;

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}