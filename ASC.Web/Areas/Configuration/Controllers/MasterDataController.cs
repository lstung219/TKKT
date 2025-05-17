using ASC.Business.Interfaces;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.Web.Areas.Configuration.Models;
using ASC.Web.Controllers;
using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace ASC.Web.Areas.Configuration.Controllers
{
    [Area("Configuration")]
    [Authorize(Roles = "Admin")]
    public class MasterDataController : BaseController
    {
        private readonly IMasterDataOperations _masterData;
        private readonly IMapper _mapper;
        public MasterDataController(IMasterDataOperations masterData, IMapper mapper)
        {
            _masterData = masterData;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> MasterKeys()
        {
            var masterKeys = await _masterData.GetAllMasterKeysAsync();
            var masterKeysViewModel = _mapper.Map<List<MasterDataKey>, List<MasterDataKeyViewModel>>(masterKeys);
            // Hold all Master Keys in session
            HttpContext.Session.SetSession("MasterKeys", masterKeysViewModel);
            return View(new MasterKeysViewModel
            {
                MasterKeys = masterKeysViewModel == null ? null : masterKeysViewModel.ToList(),
                IsEdit = false
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterKeys(MasterKeysViewModel masterKeys)
        {
            masterKeys.MasterKeys = HttpContext.Session.GetSession<List<MasterDataKeyViewModel>>("MasterKeys");
            if (!ModelState.IsValid)
            {
                return View(masterKeys);
            }
            var masterKey = _mapper.Map<MasterDataKeyViewModel, MasterDataKey>(masterKeys.MasterKeyInContext);
            if (masterKeys.IsEdit)
            {
                // Update Master Key
                await _masterData.UpdateMasterKeyAsync(masterKeys.MasterKeyInContext.PartitionKey, masterKey);
            }
            else
            {
                // Insert Master Key
                masterKey.RowKey = Guid.NewGuid().ToString();
                masterKey.PartitionKey = masterKey.Name;
                await _masterData.InsertMasterKeyAsync(masterKey);
            }
            return RedirectToAction("MasterKeys");
        }
        [HttpGet]
        public async Task<IActionResult> MasterValues()
        {
            // Get All Master Keys and hold them in ViewBag for Select tag
            ViewBag.MasterKeys = await _masterData.GetAllMasterKeysAsync();
            return View(new MasterValuesViewModel
            {
                MasterValues = new List<MasterDataValueViewModel>(),
                IsEdit = false
            });
        }

        [HttpGet]
        public async Task<IActionResult> MasterValuesByKey(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    Console.WriteLine("MasterValuesByKey called with empty key");
                    return Json(new List<MasterDataValueViewModel>());
                }

                Console.WriteLine($"MasterValuesByKey called with key: {key}");
                var masterValues = await _masterData.GetAllMasterValuesByKeyAsync(key);
                var viewModels = _mapper.Map<IEnumerable<MasterDataValue>, IEnumerable<MasterDataValueViewModel>>(masterValues);
                Console.WriteLine($"MasterValuesByKey response: {System.Text.Json.JsonSerializer.Serialize(viewModels)}");
                return Json(viewModels);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MasterValuesByKey error: {ex.Message}");
                return Json(new { Success = false, Error = $"Error retrieving data: {ex.Message}" });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterValues(bool isEdit, MasterDataValueViewModel masterValue)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { Success = false, Error = "Invalid data provided. Please check all fields." });
            }

            try
            {
                var masterDataValue = _mapper.Map<MasterDataValueViewModel, MasterDataValue>(masterValue);
                if (isEdit)
                {
                    // Update Master Value
                    await _masterData.UpdateMasterValueAsync(masterDataValue.PartitionKey, masterDataValue.RowKey, masterDataValue);
                }
                else
                {
                    // Insert Master Value
                    masterDataValue.RowKey = Guid.NewGuid().ToString();
                    masterDataValue.CreatedBy = HttpContext.User.GetCurrentUserDetails().Name; // Fixed typo: CreateBy -> CreatedBy
                    await _masterData.InsertMasterValueAsync(masterDataValue);
                }

                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = $"An error occurred: {ex.Message}" });
            }
        }
        private async Task<List<MasterDataValue>> ParseMasterDataExcel(IFormFile excelFile)
        {
            var masterValueList = new List<MasterDataValue>();
            using (var memoryStream = new MemoryStream())
            {
                // Get MemoryStream from Excel file
                await excelFile.CopyToAsync(memoryStream);
                // Create a ExcelPackage object from memory stream
                using (ExcelPackage package = new ExcelPackage(memoryStream))
                {
                    // Get the first Excel sheet from the Workbook
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    // Iterate all the rows and create the list of MasterDataValue
                    // Ignore first row as it is header
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var masterDataValue = new MasterDataValue();
                        masterDataValue.RowKey = Guid.NewGuid().ToString();
                        masterDataValue.PartitionKey = worksheet.Cells[row, 1].Value.ToString();
                        masterDataValue.Name = worksheet.Cells[row, 2].Value.ToString();
                        masterDataValue.IsActive = Boolean.Parse(worksheet.Cells[row, 3].Value.ToString());
                        masterValueList.Add(masterDataValue);
                    }
                }
            }
            return masterValueList;
        }
        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile files)
        {
            try
            {
                Console.WriteLine($"UploadExcel called with file: {(files != null ? files.FileName : "null")}, Size: {(files != null ? files.Length.ToString() : "0")}");

                if (files == null || files.Length == 0)
                {
                    return Json(new { Success = false, Error = true, Text = "No file uploaded." });
                }

                // Đọc file Excel
                var masterValues = new List<MasterDataValue>();
                using (var stream = files.OpenReadStream())
                {
                    // Giả sử sử dụng ClosedXML để đọc Excel
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();
                        var rows = worksheet.RowsUsed().Skip(1); // Bỏ qua header

                        foreach (var row in rows)
                        {
                            var masterValue = new MasterDataValue
                            {
                                PartitionKey = row.Cell(1).GetString(), // Cột 1: PartitionKey
                                Name = row.Cell(2).GetString(),         // Cột 2: Name
                                IsActive = row.Cell(3).GetBoolean(),    // Cột 3: IsActive
                                RowKey = Guid.NewGuid().ToString(),     // Tạo RowKey mới
                                IsDeleted = false,
                                CreatedBy = HttpContext.User.GetCurrentUserDetails().Name
                            };
                            masterValues.Add(masterValue);
                        }
                    }
                }

                Console.WriteLine($"Parsed {masterValues.Count} MasterDataValues from Excel: {System.Text.Json.JsonSerializer.Serialize(masterValues)}");

                // Gọi UploadBulkMasterData
                var result = await _masterData.UploadBulkMasterData(masterValues);
                if (result)
                {
                    return Json(new { Success = true });
                }
                else
                {
                    return Json(new { Success = false, Error = true, Text = "Failed to upload master data." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UploadExcel error: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Json(new { Success = false, Error = true, Text = $"Error processing file: {ex.Message}" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetMasterValue(string rowKey, string partitionKey)
        {
            try
            {
                if (string.IsNullOrEmpty(rowKey) || string.IsNullOrEmpty(partitionKey))
                {
                    return Json(new { Success = false, Error = "RowKey or PartitionKey is missing" });
                }

                Console.WriteLine($"GetMasterValue called with RowKey: {rowKey}, PartitionKey: {partitionKey}");
                var masterValue = await _masterData.GetMasterValueByNameAsync(partitionKey, rowKey);
                var viewModel = _mapper.Map<MasterDataValue, MasterDataValueViewModel>(masterValue);
                if (viewModel == null)
                {
                    return Json(new { Success = false, Error = "Master value not found" });
                }

                Console.WriteLine($"GetMasterValue response: {System.Text.Json.JsonSerializer.Serialize(viewModel)}");
                return Json(new { Success = true, MasterValue = viewModel });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetMasterValue error: {ex.Message}");
                return Json(new { Success = false, Error = $"Error retrieving data: {ex.Message}" });
            }
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> MasterValues(bool isEdit, MasterDataValueViewModel masterValue)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return Json("Error");
        //    }
        //    var masterDataValue = _mapper.Map<MasterDataValueViewModel, MasterDataValue>(masterValue);
        //    if (isEdit)
        //    {
        //        // Update Master Value
        //        await _masterData.UpdateMasterValueAsync(masterDataValue.PartitionKey, masterDataValue.RowKey, masterDataValue);
        //    }
        //    else
        //    {
        //        // Insert Master Value
        //        masterDataValue.RowKey = Guid.NewGuid().ToString();
        //        masterDataValue.CreateBy = HttpContext.User.GetCurrentUserDetails().Name;
        //        await _masterData.InsertMasterValueAsync(masterDataValue);
        //    }
        //    return Json(true);
        //}
    }
}
