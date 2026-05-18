# Invoice Manager

A Windows Forms desktop application for creating, editing, listing, exporting, and printing invoices.

Developed by **Bogdan2K2**.

## Features

- Add invoices with client, number, date, amount, and details
- View invoices in a styled grid
- Edit and delete invoices directly from the list
- Auto-maintained continuous invoice IDs for display/export
- Export all invoices to Excel (`.xlsx`)
- Generate detailed invoice documents in Word (`.docx`)
- Client logo lookup and barcode generation for invoices

## Tech Stack

- C# (.NET Framework 4.7.2)
- Windows Forms
- SQLite (`System.Data.SQLite`)
- EPPlus (Excel generation)
- Microsoft Office Interop (Word/Excel automation)
- ZXing.Net (barcode generation)

## Project Structure

- `InvoicesManager/` - Main desktop application
- `ExcelAddInInvoicesChart/` - Related Excel add-in project

## Build & Run

### Prerequisites

- Windows 10/11
- Visual Studio 2022 (or compatible MSBuild tooling)
- .NET Framework 4.7.2 Developer Pack
- Microsoft Office (for Word/Excel export features)

### Steps

1. Open `InvoicesManager/InvoicesManager.sln` in Visual Studio.
2. Restore NuGet packages.
3. Set `InvoicesManager` as startup project.
4. Build and run.

## Screenshots

Place your UI screenshots in `docs/images/` and reference them here.

Suggested files:

- `docs/images/main-dashboard.png`
- `docs/images/add-invoice-form.png`
- `docs/images/invoices-list.png`
- `docs/images/export-preview.png`

## License

This project is distributed under the End User License Agreement in [EULA.md](./EULA.md).
