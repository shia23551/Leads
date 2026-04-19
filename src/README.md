# Leads

`Leads` 是一個使用 `ABP Framework` 建立的 `ASP.NET Core MVC` 內部系統，用來維護公司聯絡資訊。

## 功能

目前已完成的核心功能如下：

* 公司資料維護
* 多租戶支援
* 使用者登入與權限控管
* 公司資料查詢、新增、編輯、刪除

公司資料欄位包含：

* 公司名稱
* 公司簡稱
* 公司統編
* 產業類別
* 員工人數
* 資本額
* 聯絡人
* 連絡電話
* 公司網址

## 查詢規則

查詢畫面支援以下條件：

* 公司名稱：`StartsWith`
* 公司簡稱：`StartsWith`
* 公司統編：`StartsWith`
* 產業類別：`Contains`
* 員工數：`=`、`>`、`<`
* 資本額：`=`、`>`、`<`

## 資料規則

* 公司統編可為空白
* 若有填寫公司統編，必須為 8 碼數字
* 同一租戶內，非空白統編不得重複
* 員工數若未提供，預設為 `0`
* 資本額若未提供，預設為 `0`

## 技術架構

* `ABP Framework`
* `ASP.NET Core MVC / Razor Pages`
* `Entity Framework Core`
* `SQL Server`
* `Layered Monolith`

專案主要組成：

* `src/Leads.Web`: Web UI
* `src/Leads.Application`: Application layer
* `src/Leads.Application.Contracts`: DTO / service contracts
* `src/Leads.Domain`: Domain layer
* `src/Leads.Domain.Shared`: shared constants / localization / permissions
* `src/Leads.EntityFrameworkCore`: EF Core / migrations
* `src/Leads.DbMigrator`: database migration and seed

## 權限

已建立以下權限：

* `Leads.Companies`
* `Leads.Companies.Create`
* `Leads.Companies.Edit`
* `Leads.Companies.Delete`

## 預設租戶

系統初始化時會建立預設租戶：

* `Share`

## 執行需求

* `.NET 10 SDK`
* `SQL Server` 或 `LocalDB`

如果要使用前端套件重建，也可安裝：

* `Node.js 18` 或 `20`

## 設定資料庫

請先確認以下檔案中的 `ConnectionStrings:Default`：

* `src/Leads.Web/appsettings.json`
* `src/Leads.DbMigrator/appsettings.json`

預設使用：

```json
Server=(LocalDb)\MSSQLLocalDB;Database=Leads;Trusted_Connection=True;TrustServerCertificate=true
```

## 初始化資料庫

在 `src/src/Leads.DbMigrator` 目錄下執行：

```bash
dotnet run
```

這會：

* 套用 EF Core migrations
* 建立基礎資料
* 建立 `Share` 租戶

## 啟動系統

在 `src/src/Leads.Web` 目錄下執行：

```bash
dotnet run
```

## 常用開發指令

在 `src` 目錄下：

```bash
dotnet build Leads.slnx
```

若需要新增 migration：

```bash
.\.tools\dotnet-ef migrations add <MigrationName> --project "src/Leads.EntityFrameworkCore/Leads.EntityFrameworkCore.csproj" --startup-project "src/Leads.DbMigrator/Leads.DbMigrator.csproj" --context LeadsDbContext
```

新增 migration 後，再執行：

```bash
dotnet run
```

位置：`src/src/Leads.DbMigrator`

## GitHub 推送前

如果要推到 GitHub，建議先確認：

* `appsettings.json` 中沒有正式環境帳密
* 沒有把憑證或敏感設定放入版本控制

## 備註

此專案目前是依「公司資料先放在同一個資料表」的需求實作，尚未拆分：

* 產業類別主檔表
* 多聯絡人子表
* Excel 匯入匯出

後續可以再依需求擴充。
