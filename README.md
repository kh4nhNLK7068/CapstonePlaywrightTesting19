# CapstonePlaywrightTesting19

Framework kiểm thử tự động cho **APP 1 - Hệ thống đặt vé xem phim** (<https://demo1.cybersoft.edu.vn/>),
xây dựng bằng **Playwright for .NET + NUnit** theo mô hình **Page Object Model (POM)**.

> Đồ án cuối khoá Testing - Cybersoft. Phần Automation: xây dựng framework bằng Playwright.

| Hạng mục | Công nghệ |
|---|---|
| Ngôn ngữ / runtime | C# · .NET 8 |
| Browser automation | Microsoft.Playwright 1.64 (Chromium / Firefox / WebKit) |
| Test runner | NUnit 3 (chạy song song theo fixture) |
| Báo cáo | ExtentReports HTML (nhúng screenshot khi fail) · NUnit TRX · Playwright Trace |
| Ghi log | Serilog (console + file log theo ngày) |
| Cấu hình | `appsettings.json` → `appsettings.{TEST_ENV}.json` → `appsettings.local.json` → biến môi trường `TEST_*` |
| Dữ liệu test | File JSON (Data-Driven Testing với `TestCaseSource`) |
| CI/CD | GitHub Actions (push, pull request, chạy hằng đêm, chạy tay có chọn browser/filter) |

---

## Mục lục

1. [Báo cáo kết quả thực thi](#1-báo-cáo-kết-quả-thực-thi)
2. [Phạm vi kiểm thử](#2-phạm-vi-kiểm-thử)
3. [Cấu trúc dự án](#3-cấu-trúc-dự-án)
4. [Kỹ thuật áp dụng](#4-kỹ-thuật-áp-dụng)
5. [Yêu cầu môi trường](#5-yêu-cầu-môi-trường)
6. [Hướng dẫn chạy dự án bằng command line](#6-hướng-dẫn-chạy-dự-án-bằng-command-line)
7. [Chạy trong các IDE khác Visual Studio](#7-chạy-trong-các-ide-khác-visual-studio)
8. [Cấu hình](#8-cấu-hình)
9. [Báo cáo, log và bằng chứng khi test fail](#9-báo-cáo-log-và-bằng-chứng-khi-test-fail)
10. [CI/CD với GitHub Actions](#10-cicd-với-github-actions)
11. [Xử lý sự cố thường gặp](#11-xử-lý-sự-cố-thường-gặp)

---

## 1. Báo cáo kết quả thực thi

**Lần chạy gần nhất:** 04/11/2026 · Chromium headless · Windows 11 · 4 worker song song

| Chỉ số | Kết quả |
|---|---|
| Tổng số test case (spreadsheet) | **47** |
| Tổng số lượt test tự động (tính cả các bộ dữ liệu của data-driven test) | **50** |
| Test không cần tài khoản - đã chạy | **41 / 41 Passed** · 0 Failed · 0 Skipped |
| Thời gian chạy 41 test | **31 giây** |
| Test cần tài khoản (category `RequiresAccount`) | 9 - chạy riêng bằng tài khoản test (xem [mục 6.4](#64-lọc-test-theo-nhóm--class--tên)) |
| Test kiểm tra lỗi đã biết (category `KnownDefect`) | 1 - **dự kiến Failed** cho tới khi ứng dụng sửa BUG-01 |

**Kết quả theo module**

| Module | Số TC | Passed | Failed | Chưa chạy (cần tài khoản) |
|---|---:|---:|---:|---:|
| Smoke / điều hướng | 6 | 6 | 0 | 0 |
| Đăng ký | 12 | 10 | 0 | 2 |
| Đăng nhập / Đăng xuất | 10 | 7 | 0 | 3 |
| Đặt vé nhanh (trang chủ) | 5 | 5 | 0 | 0 |
| Chi tiết phim | 3 | 3 | 0 | 0 |
| Chọn ghế & đặt vé | 11 | 7 | 1 (BUG-01) | 3 |
| **Tổng** | **47** | **38** | **1** | **8** |

**Lỗi phát hiện trong quá trình kiểm thử** (chi tiết ở sheet *Defects* của [`docs/TestCases.xlsx`](docs/TestCases.xlsx))

| Bug | Test case | Mô tả | Mức độ |
|---|---|---|---|
| **BUG-01** | TC_BK_11 | Đặt vé thành công thì sơ đồ ghế được tải lại và ghế vừa đặt thành “X”, nhưng **phần tóm tắt không được reset**: tổng tiền vẫn là giá ghế cũ (ví dụ `90000VND`) và mục “Chọn” vẫn còn “Ghế xx”. Nguyên nhân: reducer `movieShowtimesReducer` không xoá `bookingChairList` khi tải lại dữ liệu phòng vé. | Medium |
| **BUG-02** | TC_SU_09 | Đã nhập *Mật khẩu* nhưng bỏ trống *Nhập lại mật khẩu* thì app báo “Mật khẩu không khớp !” thay vì “Đây là trường bắt buộc !”. Test đang kiểm tra theo hành vi thực tế và ghi chú lại. | Low |

Ngoài ra, ô *Email* ở form đăng ký **không kiểm tra định dạng phía client** (ví dụ `abc` vẫn được chấp nhận). Ứng dụng chỉ kiểm tra trường bắt buộc.

Chi tiết từng test case (bước thực hiện, dữ liệu, kết quả mong đợi, script automation tương ứng, trạng thái)
nằm trong [`docs/TestCases.xlsx`](docs/TestCases.xlsx).

---

## 2. Phạm vi kiểm thử

| Module | Test case | Loại |
|---|---|---|
| Smoke / điều hướng | TC_SMK_01 - 06 | Trang chủ hiển thị, menu header, link Đăng nhập/Đăng ký, logo |
| Đăng ký | TC_SU_01 - 09 (data-driven), 10, 11, 12 | Bỏ trống trường bắt buộc, mật khẩu < 6 ký tự, nhập lại mật khẩu không khớp, họ tên chứa số, lỗi biến mất khi sửa, đăng ký thành công, trùng tài khoản (server) |
| Đăng nhập | TC_SI_01 - 04 (data-driven), 05 - 06 (data-driven, server), 07 - 10 | Bỏ trống, mật khẩu < 6 ký tự, sai tài khoản, ẩn/hiện mật khẩu, đăng nhập thành công, đăng xuất (đồng ý / huỷ) |
| Đặt vé nhanh | TC_HS_01 - 03 (data-driven), 04, 05 | Thiếu phim / rạp / suất chiếu, chọn đủ → trang chọn ghế, dropdown phụ thuộc nhau |
| Chi tiết phim | TC_MD_01 - 03 | Poster → trang chi tiết, suất chiếu → trang chọn ghế, nút “Mua vé” cuộn trang |
| Chọn ghế & đặt vé | TC_BK_01 - 11 | Sơ đồ ghế, chọn/bỏ chọn, tổng tiền, ghế đã đặt, đặt vé khi chưa đăng nhập, chưa chọn ghế, đặt 1 ghế thành công, **đặt nhiều ghế trong một lượt**, reset tóm tắt sau khi đặt vé |

Nhóm test (category) dùng để lọc khi chạy:

| Category | Ý nghĩa |
|---|---|
| `Smoke` | Kiểm tra nhanh các luồng sống còn |
| `HappyPath` | Luồng chính thành công |
| `Validation` | Validate client-side và server-side (bỏ trống, sai định dạng, trùng dữ liệu) |
| `Regression` | Các trường hợp bổ sung |
| `RequiresAccount` | Cần tài khoản thật: có đăng ký hoặc đăng nhập trên server demo |
| `KnownDefect` | Kiểm tra hành vi đúng theo đặc tả của một lỗi đã xác nhận, nên **dự kiến fail** cho tới khi ứng dụng được sửa. Loại khỏi lần chạy bằng `--filter "TestCategory!=KnownDefect"` |

---

## 3. Cấu trúc dự án

```
CapstonePlaywrightTesting19/                 ← thư mục gốc repo (chứa file .sln)
├── CapstonePlaywrightTesting19.sln
├── README.md
├── docs/
│   └── TestCases.xlsx                       # danh sách test case (import lên Google Sheets)
├── .github/workflows/
│   └── playwright-tests.yml                 # pipeline CI
└── CapstonePlaywrightTesting19/             ← project test
    ├── CapstonePlaywrightTesting19.csproj
    ├── test.runsettings                     # số worker song song, thư mục kết quả
    ├── Configuration/
    │   ├── appsettings.json                 # BaseUrl, browser, headless, timeout, artifact…
    │   ├── appsettings.local.example.json   # mẫu → copy thành appsettings.local.json (không commit)
    │   ├── ConfigManager.cs                 # đọc cấu hình nhiều lớp (JSON + biến môi trường)
    │   └── TestSettings.cs                  # model cấu hình
    ├── Fixtures/
    │   ├── GlobalSetup.cs                   # [SetUpFixture]: khởi tạo log, report, cài browser
    │   ├── BaseTest.cs                      # vòng đời Browser/Context/Page, screenshot + trace khi fail
    │   └── AccountProvider.cs               # tài khoản test: lấy từ config hoặc tự đăng ký qua API
    ├── Pages/                               # Page Object: locator + hành động, KHÔNG chứa assertion
    │   ├── BasePage.cs
    │   ├── Components/HeaderComponent.cs
    │   ├── Components/SweetAlertComponent.cs
    │   ├── HomePage.cs · SignInPage.cs · SignUpPage.cs
    │   └── MovieDetailPage.cs · PurchasePage.cs
    ├── Tests/                               # kịch bản test + assertion
    │   ├── SmokeTests.cs
    │   ├── SignUpTests.cs · SignInTests.cs
    │   ├── HomeQuickBookingTests.cs · MovieDetailTests.cs
    │   ├── BookingTests.cs
    │   └── TestCategories.cs
    ├── TestData/                            # dữ liệu data-driven + thông báo mong đợi
    │   ├── signup_validation.json · signin_validation.json · signin_server.json
    │   ├── home_quick_booking.json
    │   └── messages.json
    ├── Models/TestDataModels.cs
    └── Utilities/                           # Logger, ReportManager, JsonDataReader, DataGenerator, MovieApiClient
```

---

## 4. Kỹ thuật áp dụng

**Page Object Model & Clean Code**
- Tách 3 lớp rõ ràng: `Pages/` (locator và hành động như `LoginAsync`, `SelectAvailableSeatAsync`), `Tests/` (kịch bản và assertion), `Fixtures/` (vòng đời browser và hooks).
- Thành phần dùng chung (header, popup SweetAlert2) được tách thành *Component*.
- `BaseTest` quản lý vòng đời: mỗi fixture dùng **một browser**, mỗi test dùng **một BrowserContext mới** để cô lập cookie/localStorage. Hooks `[OneTimeSetUp]`, `[SetUp]`, `[TearDown]`, `[OneTimeTearDown]`.

**Cơ chế đồng bộ (Waits) & Locator**
- **Không dùng hard-sleep** (`Thread.Sleep`, `WaitForTimeout`). Framework dùng auto-waiting của Playwright, web-first assertion `Expect(locator).ToHaveTextAsync(...)` (tự retry) và `WaitForFunctionAsync` cho thao tác cuộn trang.
- Mỗi Page có `ReadyIndicator`, tức phần tử báo hiệu trang đã tải xong dữ liệu.
- Thứ tự ưu tiên locator: `id` (`#taiKhoan`, `#matKhau-helper-text`) → role + accessible name (`GetByRole(Button, "ĐẶT VÉ")`) → thuộc tính ngữ nghĩa (`select[name='film']`, `a[href^='/detail/']`) → class ổn định của thư viện (`.swal2-title`). **Không dùng XPath.**

**Cấu hình & dữ liệu test**
- BaseURL, loại browser, headless, timeout… nằm trong `appsettings.json` và ghi đè được bằng file local hoặc biến môi trường.
- **Data-Driven Testing:** mỗi dòng JSON trở thành một test riêng, đặt tên theo mã TC (ví dụ `TC_SU_04_Password shorter than 6 characters`).
- **Không hardcode tài khoản hay dữ liệu:** thông báo mong đợi nằm trong `messages.json`. Giá trị duy nhất được sinh lúc chạy (placeholder `{{random}}`). Tài khoản test lấy từ cấu hình hoặc tự đăng ký qua API. Suất chiếu còn ghế được tra từ API vì dữ liệu demo thay đổi liên tục.

**Báo cáo & log:** xem [mục 9](#9-báo-cáo-log-và-bằng-chứng-khi-test-fail).

**Nâng cao (Bonus):** chạy song song theo fixture (`[assembly: Parallelizable(ParallelScope.Fixtures)]`, 4 worker) và CI/CD bằng GitHub Actions.

---

## 5. Yêu cầu môi trường

| Công cụ | Phiên bản | Ghi chú |
|---|---|---|
| .NET SDK | 8.0 trở lên | Kiểm tra bằng `dotnet --list-sdks`. Nếu dùng SDK 9 thì máy cần có thêm **.NET 8 runtime** (`dotnet --list-runtimes`). |
| PowerShell | 5.1 (Windows) hoặc PowerShell 7 `pwsh` (macOS/Linux) | Dùng để chạy script cài browser `playwright.ps1`. |
| Git | bất kỳ | Để clone repo |
| Kết nối Internet | - | Tải NuGet, tải browser và truy cập trang demo |

Tải .NET SDK: <https://dotnet.microsoft.com/download/dotnet/8.0>

---

## 6. Hướng dẫn chạy dự án bằng command line

Cách này dùng được với **mọi IDE và editor** (VS Code, JetBrains Rider/IntelliJ, Vim…) và cả khi không dùng IDE.
Mọi lệnh bên dưới chạy tại **thư mục gốc repo** (nơi có file `CapstonePlaywrightTesting19.sln`), trừ khi có ghi chú khác.

### 6.1. Clone và cài đặt

**Bước 1 - Clone repo**

```bash
git clone https://github.com/kh4nhNLK7068/CapstonePlaywrightTesting19.git
```

```bash
cd CapstonePlaywrightTesting19
```

**Bước 2 - Restore package & build**

```bash
dotnet restore
```

```bash
dotnet build
```

**Bước 3 - Cài trình duyệt cho Playwright** (chỉ cần làm một lần trên mỗi máy)

Windows (PowerShell):

```powershell
powershell -ExecutionPolicy Bypass -File CapstonePlaywrightTesting19/bin/Debug/net8.0/playwright.ps1 install chromium
```

macOS / Linux (cần cài `pwsh`):

```bash
pwsh CapstonePlaywrightTesting19/bin/Debug/net8.0/playwright.ps1 install chromium
```

Linux (cài thêm thư viện hệ thống cho browser):

```bash
pwsh CapstonePlaywrightTesting19/bin/Debug/net8.0/playwright.ps1 install --with-deps chromium
```

> `GlobalSetup` cũng tự chạy `playwright install <browser>` khi bắt đầu test. Vì vậy trên máy dev, bước 3 có thể bỏ qua nếu browser tải được tự động.

### 6.2. Chạy toàn bộ test

```bash
dotnet test
```

File `test.runsettings` (4 worker song song) đã được gắn vào project, nên `dotnet test` tự dùng mà không cần thêm tham số.

Hiển thị chi tiết từng test trên console:

```bash
dotnet test --logger "console;verbosity=normal"
```

### 6.3. Liệt kê các test có trong project

```bash
dotnet test --list-tests
```

### 6.4. Lọc test theo nhóm / class / tên

Chỉ chạy nhóm Smoke:

```bash
dotnet test --filter "TestCategory=Smoke"
```

Chỉ chạy nhóm Validation:

```bash
dotnet test --filter "TestCategory=Validation"
```

Chạy tất cả **trừ** các test cần tài khoản:

```bash
dotnet test --filter "TestCategory!=RequiresAccount"
```

Chỉ chạy các test cần tài khoản:

```bash
dotnet test --filter "TestCategory=RequiresAccount"
```

Chạy một class test:

```bash
dotnet test --filter "FullyQualifiedName~BookingTests"
```

Chạy một test case theo mã TC:

```bash
dotnet test --filter "Name~TC_BK_02"
```

Kết hợp điều kiện (`&` = và, `|` = hoặc):

```bash
dotnet test --filter "TestCategory=Smoke&TestCategory!=RequiresAccount"
```

Chạy toàn bộ nhưng bỏ qua test của lỗi đã biết (ví dụ khi muốn pipeline xanh):

```bash
dotnet test --filter "TestCategory!=KnownDefect"
```

### 6.5. Đổi cấu hình khi chạy (không cần sửa code)

Mọi khoá trong `appsettings.json` đều ghi đè được bằng biến môi trường có tiền tố `TEST_`. Khoá lồng nhau nối bằng `__` (hai dấu gạch dưới).

| Mục đích | Biến môi trường |
|---|---|
| Hiện trình duyệt khi chạy | `TEST_Browser__Headless=false` |
| Chạy chậm để quan sát (ms) | `TEST_Browser__SlowMo=300` |
| Đổi trình duyệt | `TEST_Browser__Type=firefox` (hoặc `webkit`) |
| Dùng Chrome/Edge cài sẵn trên máy | `TEST_Browser__Channel=chrome` (hoặc `msedge`) |
| Tài khoản test | `TEST_TestAccount__Username=...` và `TEST_TestAccount__Password=...` |

**Windows - PowerShell** (biến chỉ có hiệu lực trong cửa sổ terminal hiện tại):

```powershell
$env:TEST_Browser__Headless = "false"; $env:TEST_Browser__SlowMo = "300"; dotnet test --filter "TestCategory=Smoke"
```

**Windows - Command Prompt (cmd):**

```bat
set TEST_Browser__Headless=false && dotnet test --filter "TestCategory=Smoke"
```

**macOS / Linux / Git Bash:**

```bash
TEST_Browser__Headless=false TEST_Browser__SlowMo=300 dotnet test --filter "TestCategory=Smoke"
```

Xoá biến sau khi dùng (PowerShell):

```powershell
Remove-Item Env:TEST_Browser__Headless, Env:TEST_Browser__SlowMo
```

### 6.6. Thay đổi số luồng chạy song song

```bash
dotnet test -- NUnit.NumberOfTestWorkers=2
```

Chạy tuần tự (1 luồng), tiện khi debug:

```bash
dotnet test -- NUnit.NumberOfTestWorkers=1
```

### 6.7. Xuất kết quả dạng TRX (cho CI hoặc công cụ khác)

```bash
dotnet test --logger "trx;LogFileName=results.trx"
```

File `.trx` nằm trong `CapstonePlaywrightTesting19/TestResults/`.

### 6.8. Chạy bản Release

```bash
dotnet build -c Release
```

```bash
dotnet test -c Release --no-build
```

Với bản Release, đường dẫn script cài browser và thư mục kết quả đổi thành `bin/Release/net8.0/...`.

---

## 7. Chạy trong các IDE khác Visual Studio

Framework là một project .NET test chuẩn, nên **mọi IDE đều chạy được bằng terminal tích hợp** với các lệnh ở [mục 6](#6-hướng-dẫn-chạy-dự-án-bằng-command-line).
Phần dưới đây hướng dẫn thêm cách chạy và debug trực tiếp trong IDE.

### 7.1. Visual Studio Code

1. Cài extension **C# Dev Kit** (Microsoft). Extension này tự cài kèm *C#* và *.NET Install Tool*.
2. `File → Open Folder…` → chọn **thư mục gốc repo** (chứa file `.sln`).
3. Mở terminal tích hợp (`Ctrl + `` ` ``) và chạy:
   ```bash
   dotnet build
   ```
4. Mở tab **Testing** (biểu tượng bình thí nghiệm ở thanh bên trái). Danh sách test NUnit hiện theo class, và bạn có thể:
   - bấm ▶ để chạy một test, một class hoặc toàn bộ;
   - bấm 🐞 (*Debug Test*) để debug với breakpoint.
5. Nếu không dùng tab Testing, chạy trong terminal tích hợp:
   ```bash
   dotnet test --filter "TestCategory=Smoke"
   ```

Mẹo: để quan sát browser khi debug, tạo file `CapstonePlaywrightTesting19/Configuration/appsettings.local.json` từ file mẫu `appsettings.local.example.json` và đặt `"Headless": false`. File này đã nằm trong `.gitignore`.

### 7.2. JetBrains Rider / IntelliJ IDEA

> IntelliJ IDEA **không hỗ trợ C#/.NET**. Với dự án .NET, JetBrains dùng **Rider** (cùng giao diện IntelliJ).
> Nếu bạn chỉ có IntelliJ IDEA, hãy dùng **terminal tích hợp** (`View → Tool Windows → Terminal`) và chạy các lệnh ở [mục 6](#6-hướng-dẫn-chạy-dự-án-bằng-command-line).

Với **Rider**:
1. `File → Open…` → chọn file `CapstonePlaywrightTesting19.sln`.
2. Đợi Rider restore NuGet xong, rồi `Build → Build Solution`.
3. Mở **Unit Tests** (`View → Tool Windows → Unit Tests`) hoặc bấm biểu tượng ▶ cạnh tên class/method trong file `Tests/*.cs` để **Run** hoặc **Debug**.
4. Để Rider dùng file `test.runsettings`: `Settings → Build, Execution, Deployment → Unit Testing → Test Runner` → bật *Use specific .runsettings file* và chọn `CapstonePlaywrightTesting19/test.runsettings`. Khi chạy bằng `dotnet test` thì runsettings được áp dụng tự động.
5. Biến môi trường (ví dụ `TEST_Browser__Headless=false`) đặt tại: `Settings → Unit Testing → Test Runner → Environment variables`.

### 7.3. Editor/terminal bất kỳ (Vim, Sublime, Notepad++…)

Chỉ cần mở terminal ở thư mục gốc repo và làm theo [mục 6](#6-hướng-dẫn-chạy-dự-án-bằng-command-line). Cách này không cần IDE.

---

## 8. Cấu hình

File chính: `CapstonePlaywrightTesting19/Configuration/appsettings.json`

| Khoá | Mặc định | Ý nghĩa |
|---|---|---|
| `BaseUrl` | `https://demo1.cybersoft.edu.vn` | Ứng dụng được kiểm thử |
| `ApiBaseUrl` | `https://movie0706.cybersoft.edu.vn/api` | Backend dùng để chuẩn bị dữ liệu (tra suất chiếu, tạo tài khoản) |
| `GroupCode` | `GP09` | Mã nhóm dữ liệu của ứng dụng |
| `Browser:Type` | `chromium` | `chromium` / `firefox` / `webkit` |
| `Browser:Channel` | *(trống)* | `chrome` / `msedge` để dùng browser cài sẵn |
| `Browser:Headless` | `true` | `false` để hiện trình duyệt |
| `Browser:SlowMo` | `0` | Độ trễ giữa các thao tác (ms), chỉ dùng khi demo |
| `Browser:ViewportWidth` / `ViewportHeight` | `1366` / `900` | Kích thước cửa sổ |
| `Timeouts:DefaultMs` / `NavigationMs` / `ExpectMs` | 15000 / 30000 / 15000 | Timeout cho thao tác / điều hướng / assertion |
| `Artifacts:OutputDir` | `TestResults` | Thư mục chứa report, log, screenshot, trace |
| `Artifacts:ScreenshotOnFailure` / `TraceOnFailure` / `VideoOnFailure` | true / true / false | Bằng chứng khi test fail |
| `TestAccount:Username` / `Password` | *(trống)* | Tài khoản cho các test cần đăng nhập |
| `TestAccount:AutoProvision` | `true` | Khi chưa cấu hình tài khoản: tự đăng ký một tài khoản mới qua API cho mỗi lần chạy |

**Thứ tự ưu tiên** (lớp sau ghi đè lớp trước):

1. `appsettings.json` - giá trị mặc định, có commit.
2. `appsettings.{TEST_ENV}.json` - theo môi trường (ví dụ `TEST_ENV=staging` → `appsettings.staging.json`).
3. `appsettings.local.json` - cấu hình riêng trên máy, **không commit** (đã có trong `.gitignore`).
4. Biến môi trường `TEST_*`.

**Cấu hình tài khoản test (không đưa lên Git):** copy file mẫu rồi điền tài khoản của bạn.

Windows (PowerShell):

```powershell
Copy-Item CapstonePlaywrightTesting19/Configuration/appsettings.local.example.json CapstonePlaywrightTesting19/Configuration/appsettings.local.json
```

macOS / Linux:

```bash
cp CapstonePlaywrightTesting19/Configuration/appsettings.local.example.json CapstonePlaywrightTesting19/Configuration/appsettings.local.json
```

Sau khi copy, sửa `Username`/`Password` trong `appsettings.local.json`, rồi **build lại** (file cấu hình được copy vào thư mục `bin` khi build).

---

## 9. Báo cáo, log và bằng chứng khi test fail

Sau mỗi lần chạy, kết quả nằm tại `CapstonePlaywrightTesting19/bin/Debug/net8.0/TestResults/` (bản Release: `bin/Release/...`):

| Đường dẫn | Nội dung |
|---|---|
| `Reports/TestReport.html` | **Báo cáo HTML (ExtentReports):** tổng quan pass/fail, các bước của từng test, category, thông tin môi trường; **ảnh chụp màn hình nhúng trực tiếp** vào test bị fail |
| `Logs/test-run-YYYYMMDD.log` | Log chi tiết từng bước của mọi test (mật khẩu được che bằng `***`) |
| `Screenshots/<tên test>.png` | Ảnh chụp toàn trang tại thời điểm test fail |
| `Traces/<tên test>.zip` | Playwright Trace của test fail: DOM snapshot, network, từng thao tác |

Mở báo cáo HTML:

```powershell
start CapstonePlaywrightTesting19/bin/Debug/net8.0/TestResults/Reports/TestReport.html
```

```bash
open CapstonePlaywrightTesting19/bin/Debug/net8.0/TestResults/Reports/TestReport.html
```

(`start` cho Windows, `open` cho macOS, `xdg-open` cho Linux.)

Xem Playwright Trace của một test fail (thay tên file cho phù hợp):

```bash
pwsh CapstonePlaywrightTesting19/bin/Debug/net8.0/playwright.ps1 show-trace CapstonePlaywrightTesting19/bin/Debug/net8.0/TestResults/Traces/TC_SMK_01_HomePageLoads.zip
```

**Demo cơ chế chụp màn hình khi fail:** cố tình trỏ `BaseUrl` sang ứng dụng khác để test fail, rồi mở report:

```powershell
$env:TEST_BaseUrl = "https://demo2.cybersoft.edu.vn"; dotnet test --filter "Name~TC_SMK_01"; Remove-Item Env:TEST_BaseUrl
```

---

## 10. CI/CD với GitHub Actions

Workflow: [`.github/workflows/playwright-tests.yml`](.github/workflows/playwright-tests.yml)

| Sự kiện kích hoạt | Mô tả |
|---|---|
| `push` / `pull_request` vào `main` | Chạy toàn bộ test trên Chromium |
| `schedule` | Chạy hồi quy hằng đêm lúc 08:00 giờ Việt Nam |
| `workflow_dispatch` | Chạy tay từ tab **Actions** → *Run workflow*, chọn được **browser** (chromium/firefox/webkit) và **filter** (ví dụ `TestCategory=Smoke`) |

Các bước trong pipeline: checkout → cài .NET 8 → build Release → cài browser và thư viện hệ thống → `dotnet test` → publish kết quả NUnit thành *check run* → upload **HTML report, log, screenshot, trace** thành artifact (giữ 14 ngày).

Muốn dùng tài khoản cố định trong CI thay vì tự đăng ký: vào `Settings → Secrets and variables → Actions` và tạo hai secret `TEST_ACCOUNT_USERNAME`, `TEST_ACCOUNT_PASSWORD`.

---

## 11. Xử lý sự cố thường gặp

| Hiện tượng | Cách xử lý |
|---|---|
| `Executable doesn't exist at ...ms-playwright...` | Chưa cài browser. Chạy lại bước 3 ở [mục 6.1](#61-clone-và-cài-đặt). |
| `playwright.ps1` không tồn tại | Chưa build. Chạy `dotnet build` trước. |
| `You must install or update .NET to run this application` (framework 8.0) | Cài **.NET 8 runtime/SDK**. |
| PowerShell báo *running scripts is disabled* | Chạy script bằng `powershell -ExecutionPolicy Bypass -File ...` như ở bước 3. |
| `spawn UNKNOWN` khi mở Firefox/WebKit trên Windows | Phần mềm diệt virus hoặc sandbox chặn browser. Dùng `chromium` hoặc `TEST_Browser__Channel=msedge`. |
| Test `RequiresAccount` bị *Ignored* | `AutoProvision=false` mà chưa cấu hình tài khoản. Hãy điền tài khoản ([mục 8](#8-cấu-hình)) hoặc bật lại `AutoProvision`. |
| Test đặt vé báo *Inconclusive* | Server demo không còn suất chiếu nào trống ghế. Thêm mã suất chiếu khác vào `PreferredShowtimeIds` trong `TestData/messages.json`. |
| Test chập chờn do mạng chậm | Tăng `Timeouts:ExpectMs` / `NavigationMs`, ví dụ `TEST_Timeouts__ExpectMs=30000`. |
| Sửa `appsettings.local.json` nhưng không có tác dụng | Build lại để file được copy vào `bin/`. |
