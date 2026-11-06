# Hướng Dẫn Luồng Thực Thi Dự Án CapstonePlaywrightTesting19

## Giới Thiệu Chung

**CapstonePlaywrightTesting19** là một dự án **Automated Testing (Kiểm thử tự động)** được xây dựng với:
- **Ngôn ngữ:** C# (.NET 8)
- **Framework Testing:** NUnit 3.14.0
- **Công cụ tự động hoá:** Playwright 1.64.1
- **Mục tiêu:** Kiểm thử ứng dụng web phim ticket booking

---

## 🎯 Mục Đích Của Dự Án

Kiểm thử chức năng của ứng dụng web theo hình thức **automated testing**:
- Kiểm thử đăng nhập/đăng ký
- Kiểm thử chọn ghế và đặt vé xem phim
- Kiểm thử tìm kiếm phim
- Kiểm thử giao diện người dùng

---

## 📁 Cấu Trúc Dự Án

```
CapstonePlaywrightTesting19/
├── Configuration/                    # ⚙️ Cấu hình
│   ├── appsettings.json             # Cài đặt mặc định
│   ├── appsettings.local.json       # Cài đặt cục bộ (git-ignore)
│   ├── ConfigManager.cs             # Quản lý tải cấu hình
│   └── TestSettings.cs              # Lớp cấu hình kiểu mạnh
│
├── Fixtures/                         # 🔧 Khởi tạo/Dọn dẹp
│   ├── GlobalSetup.cs               # Setup toàn bộ test suite
│   ├── BaseTest.cs                  # Lớp cơ sở cho tất cả test
│   └── AccountProvider.cs           # Cung cấp tài khoản test
│
├── Tests/                            # 🧪 Các test case
│   ├── TestCategories.cs            # Phân loại test
│   ├── BookingTests.cs              # Test đặt vé
│   ├── SignInTests.cs               # Test đăng nhập
│   ├── SignUpTests.cs               # Test đăng ký
│   ├── MovieDetailTests.cs          # Test chi tiết phim
│   ├── SmokeTests.cs                # Test khói (smoke)
│   └── HomeQuickBookingTests.cs     # Test đặt vé nhanh
│
├── Pages/                            # 📄 Page Object Pattern
│   ├── BasePage.cs                  # Lớp cơ sở cho các page
│   ├── HomePage.cs                  # Trang chủ
│   ├── SignInPage.cs                # Trang đăng nhập
│   ├── SignUpPage.cs                # Trang đăng ký
│   ├── MovieDetailPage.cs           # Trang chi tiết phim
│   ├── PurchasePage.cs              # Trang mua vé
│   └── Components/                  # Các component UI
│       ├── HeaderComponent.cs       # Component thanh điều hướng
│       └── SweetAlertComponent.cs   # Component cảnh báo
│
├── Models/                           # 📊 Mô hình dữ liệu
│   └── TestDataModels.cs            # Các lớp dữ liệu test
│
├── Utilities/                        # 🛠️ Tiện ích hỗ trợ
│   ├── Logger.cs                    # Ghi log
│   ├── ReportManager.cs             # Quản lý báo cáo HTML
│   ├── MovieApiClient.cs            # Gọi API phim
│   ├── DataGenerator.cs             # Tạo dữ liệu ngẫu nhiên
│   └── JsonDataReader.cs            # Đọc dữ liệu JSON
│
├── TestData/                         # 📋 Dữ liệu test tĩnh
│   └── *.json                        # File JSON chứa dữ liệu
│
├── CapstonePlaywrightTesting19.csproj # Tệp dự án C#
├── test.runsettings                  # Cấu hình chạy test
└── TestResults/                      # 📊 Kết quả test (được tạo)
    ├── Reports/                      # Báo cáo HTML
    ├── Screenshots/                  # Ảnh chụp khi test fail
    ├── Traces/                       # Trace Playwright
    ├── Videos/                       # Video khi test fail
    └── Logs/                         # File log
```

---

## 🚀 Luồng Thực Thi Chi Tiết

### **Bước 1: Khởi động (Startup)**

Khi bạn chạy lệnh `dotnet test` hoặc chạy test từ IDE:

```
dotnet test
```

---

### **Bước 2: GlobalSetup (Chạy MỘT lần cho toàn bộ test suite)**

**File:** `Fixtures/GlobalSetup.cs`

```csharp
[assembly: Parallelizable(ParallelScope.Fixtures)]
[SetUpFixture]
public sealed class GlobalSetup
{
    [OneTimeSetUp]
    public void BeforeAll()
    {
        // 1. Tải cấu hình từ appsettings.json
        var settings = ConfigManager.Settings;
        
        // 2. Tạo thư mục output: TestResults/
        OutputDir = ...
        
        // 3. Khởi tạo Logger (ghi log)
        Logger.Init(OutputDir + "/Logs");
        
        // 4. Khởi tạo ReportManager (báo cáo HTML)
        ReportManager.Init(OutputDir + "/Reports");
        
        // 5. In thông tin bắt đầu
        Logger.Info("Test run started - https://demo1.cybersoft.edu.vn on chromium...")
        
        // 6. ⭐ CỰC KỲ QUAN TRỌNG: Tải browser binaries (Chromium/Firefox/WebKit)
        //    Chỉ tải 1 lần, sau đó được tái sử dụng
        Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
    }

    [OneTimeTearDown]
    public void AfterAll()
    {
        // Khi tất cả test xong:
        ReportManager.Flush();  // Lưu báo cáo HTML
        Logger.Close();          // Đóng file log
    }
}
```

**Các công việc thực hiện:**
| Công việc | Thực hiện | Mục đích |
|----------|----------|---------|
| Tải cấu hình | 1 lần | Đọc `appsettings.json`, env vars |
| Khởi tạo Logger | 1 lần | Tạo file log |
| Khởi tạo Report | 1 lần | Tạo báo cáo HTML |
| **Tải Browser** | **1 lần** | **Tải Chromium/Firefox binaries** |

---

### **Bước 3: Khởi động Test Fixtures (Một browser cho mỗi Test Class)**

**File:** `Fixtures/BaseTest.cs`

Một **test class** (ví dụ: `BookingTests`) = Một **fixture**

Với cấu hình `Parallelizable(ParallelScope.Fixtures)`:
- Mỗi fixture chạy với **một browser instance riêng**
- Các fixture chạy **song song** (4 fixtures cùng lúc - theo `<NumberOfTestWorkers>4</NumberOfTestWorkers>`)

```csharp
public abstract class BaseTest
{
    private IBrowser _browser;
    protected IPage Page;

    [OneTimeSetUp]  // ⭐ Chạy MỘT lần cho TOÀN BỘ fixture (test class)
    public async Task LaunchBrowserAsync()
    {
        // 1. Tạo Playwright instance
        _playwright = await Playwright.CreateAsync();
        
        // 2. Chọn browser type (chromium/firefox/webkit)
        var browserType = _playwright.Chromium;
        
        // 3. Launch browser với cấu hình từ appsettings.json
        _browser = await browserType.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,        // Chạy không giao diện
            SlowMo = 0,             // Không chậm
            Channel = null          // Dùng browser mặc định
        });
        
        Logger.Info($"Browser launched for fixture {GetType().Name}");
    }

    [OneTimeTearDown]  // Chạy MỘT lần ở cuối
    public async Task CloseBrowserAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }
}
```

**Quy trình:**
```
Test Suite chạy
│
├─ OneTimeSetUp (GlobalSetup.BeforeAll)
│  └─ Tải browser binaries (Chromium)
│
├─ Fixture 1 (BookingTests)          ┐
│  ├─ OneTimeSetUp                   │ Chạy song song
│  │  └─ Launch Browser (Chomium)    │ (4 fixtures cùng lúc)
│  ├─ Test TC_BK_01                  │
│  ├─ Test TC_BK_02                  │
│  └─ OneTimeTearDown                │
│     └─ Close Browser               ┘
│
├─ Fixture 2 (SignInTests)           ┐
│  ├─ OneTimeSetUp                   │
│  │  └─ Launch Browser (Chromium)   │ (4 fixtures cùng lúc)
│  ├─ Test ...                       │
│  └─ OneTimeTearDown                ┘
│
├─ Fixture 3, 4, ... (các test class khác)
│
└─ OneTimeTearDown (GlobalSetup.AfterAll)
   └─ Lưu báo cáo HTML
```

---

### **Bước 4: Chạy từng Test (Setup + Teardown cho mỗi test)**

**File:** `Fixtures/BaseTest.cs`

Cho mỗi test method:

```csharp
[SetUp]  // ⭐ Chạy TRƯỚC mỗi test
public async Task CreateContextAsync()
{
    // 1. Tạo context mới (phiên làm việc trình duyệt cách ly)
    Context = await _browser.NewContextAsync(new BrowserNewContextOptions
    {
        BaseURL = "https://demo1.cybersoft.edu.vn",
        Locale = "vi-VN",
        ViewportSize = { Width = 1366, Height = 900 },
        RecordVideoDir = null  // Hoặc video/path nếu muốn
    });
    
    // 2. Bắt đầu ghi Trace (record hoạt động)
    await Context.Tracing.StartAsync(...);
    
    // 3. Tạo trang mới
    Page = await Context.NewPageAsync();
    
    // 4. Ghi log bắt đầu test
    Logger.Info("===== START =====");
}

[TearDown]  // ⭐ Chạy SAU mỗi test
public async Task CloseContextAsync()
{
    try
    {
        // 1. Nếu test FAIL:
        if (testFailed)
        {
            // a) Chụp ảnh màn hình
            await Page.ScreenshotAsync("Screenshots/test_name.png");
            
            // b) Lưu Trace Playwright
            await Context.Tracing.StopAsync("Traces/test_name.zip");
            
            // c) Ghi log lỗi
            Logger.Error("===== FAILED =====");
        }
        
        // 2. Nếu test PASS:
        else
        {
            Logger.Info("===== PASSED =====");
        }
    }
    finally
    {
        // 3. Đóng context (dọn dẹp)
        await Context.CloseAsync();
    }
}
```

**Mục đích:**
- Mỗi test có **context riêng** (cách ly)
- Khi fail → chụp ảnh + lưu trace để debug
- Khi pass → dọn dẹp dữ liệu

---

### **Bước 5: Test Execution (Chạy test method)**

**Ví dụ:** `Tests/BookingTests.cs`

```csharp
[TestFixture]
public sealed class BookingTests : BaseTest
{
    [OneTimeSetUp]
    public async Task FindShowtimeAsync()
    {
        // Chạy MỘT lần TRƯỚC tất cả test trong class này
        using var api = new MovieApiClient();
        _showtimeId = await api.FindShowtimeWithFreeSeatsAsync(...);
    }

    [Test(Description = "TC_BK_02 - Selecting a seat highlights it")]
    [Category(TestCategories.HappyPath)]
    public async Task TC_BK_02_SelectSeatUpdatesTotal()
    {
        // 1. Mở trang mua vé
        var purchase = await OpenPurchasePageAsync();
        
        // 2. Chọn ghế
        var seat = await purchase.SelectAvailableSeatAsync();
        
        // 3. Kiểm tra: ghế được highlight màu xanh
        await Expect(purchase.Seat(seat))
            .ToHaveCSSAsync("background-color", "rgb(0, 128, 0)");
        
        // 4. Kiểm tra: tổng tiền không phải 0
        await Expect(purchase.TotalPrice)
            .Not.ToHaveTextAsync("0VND");
    }
}
```

---

## ⚙️ Tệp Cấu Hình: `test.runsettings`

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <RunConfiguration>
    <ResultsDirectory>./TestResults</ResultsDirectory>
  </RunConfiguration>
  <NUnit>
    <!-- Fixtures run in parallel -->
    <NumberOfTestWorkers>4</NumberOfTestWorkers>
  </NUnit>
</RunSettings>
```

### 📌 Giải Thích `<NumberOfTestWorkers>4</NumberOfTestWorkers>`

**Tham số này cho biết: Bao nhiêu Test Fixtures (Test Classes) được chạy song song cùng một lúc?**

| Tham số | Nghĩa | Ví dụ |
|--------|--------|--------|
| `<NumberOfTestWorkers>4</NumberOfTestWorkers>` | Chạy **tối đa 4 fixtures** cùng lúc | Nếu có 10 test classes, thì 4 cái chạy đầu tiên, khi xong rồi 4 cái tiếp theo, ... |

#### **Cơ Chế Hoạt Động:**

```
📊 Ví dụ: 6 test classes (fixtures)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Timeline (Thời gian):
├─ Thời điểm 0s:
│  BookingTests (Browser 1)
│  SignInTests (Browser 2)
│  SignUpTests (Browser 3)
│  MovieDetailTests (Browser 4)
│  └─ Chạy song song ✅
│
├─ Thời điểm 45s (khi 4 fixtures trên xong):
│  HomeQuickBookingTests (Browser 5)
│  SmokeTests (Browser 6)
│  └─ Chạy song song ✅
│
└─ Kết thúc: ~90s (không phải 45s × 6 = 270s)
```

#### **Tại Sao Cần `NumberOfTestWorkers`?**

| Lý Do | Giải Thích |
|------|-----------|
| **⚡ Tăng tốc độ** | Thay vì chạy tuần tự (1 → 2 → 3), chạy song song (1,2,3,4 cùng lúc) |
| **💪 Tận dụng CPU** | PC hiện đại có multi-core, không lý do để chạy 1 test khi có thể chạy 4 test |
| **⚖️ Cân bằng** | Không nên quá cao (4-8) vì gây tải máy; không nên quá thấp (1-2) vì chậm |

#### **Điểm Quan Trọng:**

> ⭐ **Fixtures (Test Classes) chạy SONG SONG, nhưng Tests trong CÙNG MỘT Fixture chạy TUẦN TỰ**

```
BookingTests (Fixture 1)
├─ TC_BK_01 → TC_BK_02 → TC_BK_03 (TUẦN TỰ - dùng chung Browser 1)
└─ Chia sẻ 1 browser

SignInTests (Fixture 2)  
├─ SignIn_01 → SignIn_02 (TUẦN TỰ - dùng chung Browser 2)
└─ Chia sẻ 1 browser

(Cả 2 Fixture chạy SONG SONG với nhau)
```

---

## 📋 Quy Trình Chi Tiết Của Test Chạy

Dưới đây là sơ đồ chi tiết của quá trình **một test chạy từ đầu đến cuối**:

```
Test Execution Flow (Luồng thực thi một test)
═════════════════════════════════════════════

1️⃣ KHỞI ĐỘNG TOÀN BỘ TEST SUITE
   ├─ GlobalSetup.BeforeAll() [OneTimeSetUp]
   │  ├─ Tải cấu hình từ appsettings.json
   │  ├─ Tạo thư mục TestResults/, Logs/, Reports/
   │  ├─ Khởi tạo Logger (ghi log)
   │  ├─ Khởi tạo ReportManager (báo cáo HTML)
   │  └─ Tải Playwright Browser Binaries (Chromium/Firefox/WebKit)
   │     💾 Được lưu vào cache, chỉ tải 1 lần dù chạy test nhiều lần
   │
   └─ ✅ GlobalSetup hoàn thành

2️⃣ MỖI TEST CLASS (FIXTURE) KHỞI ĐỘNG
   └─ BaseTest.LaunchBrowserAsync() [OneTimeSetUp]
      ├─ Tạo Playwright instance
      ├─ Chọn browser type (chromium/firefox/webkit)
      ├─ Launch Browser với cấu hình:
      │  ├─ Headless = true (chạy không giao diện)
      │  ├─ SlowMo = 0 (không chậm)
      │  └─ Channel = null (browser mặc định)
      ├─ Loại bỏ các lỗi cảnh báo từ trình duyệt
      └─ 1 Browser instance được dùng cho TẤT CẢ test trong class này

3️⃣ CHUẨN BỊ TẠM THỜI TRƯỚC MỖI TEST
   └─ BaseTest.CreateContextAsync() [SetUp]
      ├─ Ghi log: "===== START ====="
      ├─ Tạo BrowserContext (phiên làm việc cách ly)
      │  ├─ BaseURL = "https://demo1.cybersoft.edu.vn"
      │  ├─ Locale = "vi-VN"
      │  ├─ ViewportSize = 1366×900
      │  └─ RecordVideoDir = null (hoặc path nếu muốn video)
      ├─ Bắt đầu ghi Trace (record click, input, navigation, ...)
      ├─ Tạo Page (trang web)
      └─ ✅ Sẵn sàng chạy test

4️⃣ CHẠY TEST METHOD (LOGIC TEST CHÍNH)
   └─ TC_BK_02_SelectSeatUpdatesTotal()
      ├─ var purchase = await OpenPurchasePageAsync()
      │  └─ Điều hướng đến https://demo1.cybersoft.edu.vn/purchase/123
      ├─ var seat = await purchase.SelectAvailableSeatAsync()
      │  ├─ Tìm ghế trống (locator)
      │  ├─ Click vào ghế
      │  └─ Trả về tên ghế (VD: "A1")
      ├─ await Expect(purchase.Seat(seat)).ToHaveCSSAsync(...)
      │  └─ Kiểm tra: ghế có màu xanh không?
      ├─ await Expect(purchase.TotalPrice).Not.ToHaveTextAsync("0VND")
      │  └─ Kiểm tra: tổng tiền không phải 0 không?
      └─ ✅ Test hoàn thành (PASS nếu tất cả assertion OK)

5️⃣ DỌN DẸP NGAY SAU MỖI TEST
   └─ BaseTest.CloseContextAsync() [TearDown]
      ├─ Lấy kết quả test (Pass/Fail/Skip)
      │
      ├─ NẾU FAIL:
      │  ├─ Chụp ảnh màn hình:
      │  │  └─ await Page.ScreenshotAsync("TestResults/Screenshots/test_name.png")
      │  ├─ Lưu Trace (để replay lại hoạt động):
      │  │  └─ await Context.Tracing.StopAsync("TestResults/Traces/test_name.zip")
      │  ├─ Attach files vào NUnit report
      │  ├─ Ghi log: "===== FAILED ====="
      │  └─ Ghi lỗi vào HTML Report (ReportManager)
      │
      ├─ NẾU PASS:
      │  ├─ Ghi log: "===== PASSED ====="
      │  └─ Ghi Pass vào HTML Report
      │
      └─ Đóng Context (dọn dẹp dữ liệu tạm, cookie, storage, ...)

6️⃣ CHUẨN BỊ CHO TEST TIẾP THEO
   └─ (Quay lại bước 3, với Context/Page mới)

7️⃣ KỌI TOÀN BỘ TEST CLASS XONG
   └─ BaseTest.CloseBrowserAsync() [OneTimeTearDown]
      ├─ Đóng Browser
      └─ Dispose Playwright instance

8️⃣ TẮT TOÀN BỘ TEST SUITE
   └─ GlobalSetup.AfterAll() [OneTimeTearDown]
      ├─ Flush HTML Report:
      │  └─ Ghi file index.html vào TestResults/Reports/
      ├─ Ghi log cuối: "Test run finished"
      └─ Đóng Logger (đóng file log)

✅ TEST SUITE HOÀN THÀNH
   Kết quả:
   ├─ TestResults/Reports/index.html (báo cáo chi tiết)
   ├─ TestResults/Logs/*.log (file log)
   ├─ TestResults/Screenshots/*.png (ảnh khi fail)
   └─ TestResults/Traces/*.zip (trace khi fail)
```

---

## 🔄 Lifecycle Diagram

```
Assembly-level (Toàn bộ project):
┌────────────────────────────────────────────────────────┐
│ GlobalSetup.BeforeAll() [OneTimeSetUp]                │
│ ├─ Tải cấu hình, Logger, Report                       │
│ └─ Cài đặt Playwright Browser                         │
└────────────────────────────────────────────────────────┘

Fixture-level (Mỗi Test Class) × 4 song song:
┌────────────────────────────────────────────────────────┐
│ BaseTest.LaunchBrowserAsync() [OneTimeSetUp]          │
│ └─ Launch 1 Browser cho fixture này                   │
└────────────────────────────────────────────────────────┘

Test-level (Mỗi Test Method) × N tuần tự:
┌────────────────────────────────────────────────────────┐
│ BaseTest.CreateContextAsync() [SetUp]                 │
│ ├─ Tạo Context/Page mới (cách ly)                     │
│ └─ Bắt đầu ghi Trace                                  │
├────────────────────────────────────────────────────────┤
│ TEST METHOD (Logic chính)                              │
├────────────────────────────────────────────────────────┤
│ BaseTest.CloseContextAsync() [TearDown]               │
│ ├─ Nếu fail: chụp ảnh + lưu trace                     │
│ └─ Đóng Context                                        │
└────────────────────────────────────────────────────────┘

Fixture-level (Khi fixture xong):
┌────────────────────────────────────────────────────────┐
│ BaseTest.CloseBrowserAsync() [OneTimeTearDown]        │
│ └─ Đóng Browser                                        │
└────────────────────────────────────────────────────────┘

Assembly-level (Toàn bộ project xong):
┌────────────────────────────────────────────────────────┐
│ GlobalSetup.AfterAll() [OneTimeTearDown]              │
│ ├─ Lưu HTML Report                                    │
│ └─ Đóng Logger                                         │
└────────────────────────────────────────────────────────┘
```

---

## 🎯 Phân Loại Test (TestCategories)

```csharp
// Dùng để filter test khi chạy
[Test]
[Category(TestCategories.Smoke)]          // Test khí (chạy nhanh, kiểm tra cơ bản)
[Category(TestCategories.Regression)]     // Test hồi quy (full flow)
[Category(TestCategories.Validation)]     // Test validate input/output
[Category(TestCategories.HappyPath)]      // Test happy path (success case)
[Category(TestCategories.RequiresAccount)]// Cần tài khoản (chạy LoginAsync trước)
[Category(TestCategories.KnownDefect)]    // Known bug (expected to fail)
public async Task TestMethod() { }

// Chạy chỉ Smoke tests:
// dotnet test --filter "TestCategory=Smoke"
```

---

## 🔐 Quản Lý Tài Khoản Test

**File:** `Fixtures/AccountProvider.cs`

```csharp
// Luồng:
1. Nếu appsettings.local.json có tài khoản
   → Dùng tài khoản đó
   
2. Nếu không, và AutoProvision = true
   → Tạo tài khoản tạm thời qua API
   → Test dùng tài khoản tạo
   
3. Nếu không cấu hình và AutoProvision = false
   → Test skip (Inconclusive)

// Cách dùng trong test:
[Test]
[Category(TestCategories.RequiresAccount)]
public async Task BookTicket()
{
    var account = await LoginAsync();  // Dùng AccountProvider bên trong
    // ...
}
```

---

## 📊 Cấu Hình (appsettings.json)

```json
{
  "BaseUrl": "https://demo1.cybersoft.edu.vn",
  "ApiBaseUrl": "https://movie0706.cybersoft.edu.vn/api",
  
  "Browser": {
    "Type": "chromium",           // chromium | firefox | webkit
    "Headless": true,             // true = không giao diện, false = hiện giao diện
    "SlowMo": 0,                  // ms delay giữa các action (0 = không delay)
    "ViewportWidth": 1366,        // Chiều rộng viewport
    "ViewportHeight": 900         // Chiều cao viewport
  },
  
  "Timeouts": {
    "DefaultMs": 15000,           // Timeout mặc định (15 giây)
    "NavigationMs": 30000,        // Timeout cho navigation (30 giây)
    "ExpectMs": 15000             // Timeout cho expect assertion (15 giây)
  },
  
  "Artifacts": {
    "ScreenshotOnFailure": true,  // Chụp ảnh khi test fail
    "TraceOnFailure": true,       // Lưu trace khi test fail
    "VideoOnFailure": false       // Ghi video khi test fail (chậm, không recommend)
  },
  
  "TestAccount": {
    "AutoProvision": true         // Tạo tài khoản tạm thời nếu không cấu hình
  }
}
```

---

## 🚀 Cách Chạy Test

### Chạy tất cả test:
```bash
dotnet test
```

### Chạy chỉ test Smoke:
```bash
dotnet test --filter "TestCategory=Smoke"
```

### Chạy chỉ BookingTests:
```bash
dotnet test --filter "ClassName=BookingTests"
```

### Chạy chỉ 1 test:
```bash
dotnet test --filter "Name=TC_BK_02_SelectSeatUpdatesTotal"
```

### Chạy với cấu hình cụ thể:
```bash
TEST_Browser__Headless=false dotnet test
# Sẽ chạy với giao diện (không headless)
```

---

## 🔧 Điều Chỉnh `NumberOfTestWorkers`

### Tăng song song (chạy nhanh hơn, tải máy cao):
```xml
<NumberOfTestWorkers>8</NumberOfTestWorkers>
```

### Giảm song song (chạy chậm hơn, tiết kiệm tài nguyên):
```xml
<NumberOfTestWorkers>2</NumberOfTestWorkers>
```

### Chạy tuần tự (1 test class một lúc):
```xml
<NumberOfTestWorkers>1</NumberOfTestWorkers>
```

---

## 📈 Kết Quả Test

Sau khi chạy `dotnet test`, bạn sẽ tìm thấy:

```
TestResults/
├── index.html                      # 📊 Báo cáo HTML chi tiết (mở bằng trình duyệt)
├── Logs/
│   └── test_log_*.log              # 📝 File log text
├── Screenshots/
│   └── Test_name.png               # 📸 Ảnh khi test fail
├── Traces/
│   └── Test_name.zip               # 🎬 Trace Playwright (replay)
└── Videos/
    └── Test_name.webm              # 📹 Video khi test fail (nếu enable)
```

---

## 💡 Ví Dụ Minh Hoạ: Chạy BookingTests

```
Bước 1: GlobalSetup.BeforeAll()
  └─ Setup Logger, Report, Tải Chromium

Bước 2: BookingTests.LaunchBrowserAsync() [OneTimeSetUp]
  └─ Launch 1 Chromium browser cho BookingTests

Bước 3: Chạy TC_BK_01_SeatMapDisplayed
  ├─ Setup: Tạo Context/Page mới, Bắt Trace
  ├─ Test: Mở /purchase/{showtimeId}, kiểm tra UI
  └─ Teardown: Đóng Context
       → PASS ✅

Bước 4: Chạy TC_BK_02_SelectSeatUpdatesTotal
  ├─ Setup: Tạo Context/Page mới, Bắt Trace
  ├─ Test: Chọn ghế, kiểm tra highlight + giá
  └─ Teardown: Đóng Context
       → PASS ✅

Bước 5: Chạy TC_BK_03_DeselectSeatResetsTotal
  ├─ Setup: Tạo Context/Page mới, Bắt Trace
  ├─ Test: Chọn rồi bỏ chọn ghế
  └─ Teardown: Đóng Context
       → FAIL ❌ (Chụp ảnh, Lưu Trace)

... (các test tiếp theo) ...

Bước N: BookingTests.CloseBrowserAsync() [OneTimeTearDown]
  └─ Đóng Chromium browser

Bước N+1: Các fixture khác chạy (SignInTests, SignUpTests, ...)
  └─ Mỗi fixture có browser riêng, chạy song song

Bước Cuối: GlobalSetup.AfterAll()
  └─ Lưu HTML Report, Đóng Logger
  └─ Xong! ✅
```

---

## 🎓 Tóm Tắt Cho Newbie

| Khái Niệm | Giải Thích | Ví Dụ |
|----------|-----------|--------|
| **Test Fixture** | Một lớp test (class) = 1 fixture | `BookingTests`, `SignInTests` |
| **NumberOfTestWorkers=4** | Tối đa 4 fixtures chạy song song | Nếu 10 fixtures, 4 cái đầu chạy, xong rồi 4 cái tiếp |
| **Parallelizable(ParallelScope.Fixtures)** | Cho phép fixtures chạy song song | Tận dụng multi-core CPU |
| **OneTimeSetUp** | Chạy 1 lần ở đầu (trước tất cả test) | Tạo browser cho fixture |
| **SetUp** | Chạy trước mỗi test | Tạo page, context mới |
| **Test Method** | Test chính (logic kiểm thử) | Chọn ghế, kiểm tra giá |
| **TearDown** | Chạy sau mỗi test | Chụp ảnh fail, dọn dẹp |
| **OneTimeTearDown** | Chạy 1 lần ở cuối (sau tất cả test) | Đóng browser |
| **Context** | Phiên làm việc trình duyệt (cách ly) | Mỗi test có context riêng → không ảnh hưởng nhau |
| **Page** | Trang web (tab) | Dùng để navigate, click, type, assert |
| **Trace** | Ghi lại hoạt động (click, input, ...) | Dùng để debug test fail |
| **Headless** | Chạy không giao diện | `true` = nhanh, `false` = chậm nhưng thấy được |

---

## 🎯 Kết Luận

Dự án **CapstonePlaywrightTesting19** là một **test automation framework** hoàn chỉnh với:

✅ **Cấu trúc rõ ràng**: Tách biệt fixtures, tests, pages, utilities  
✅ **Quản lý tài nguyên tốt**: Browser được tái sử dụng, context/page cách ly  
✅ **Báo cáo chi tiết**: HTML report, screenshots, traces, logs  
✅ **Chạy song song**: Tăng tốc độ với `NumberOfTestWorkers=4`  
✅ **Dễ mở rộng**: Thêm test class mới chỉ cần extends `BaseTest`  

Hy vọng hướng dẫn này giúp bạn hiểu rõ luồng thực thi dự án! 🚀
