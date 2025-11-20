# ToolBoxV2

ToolBoxV2 is a WPF MVVM application built as a **CLEAN Architecture learning project**.  
Its main purpose is to work with:

- **XML files** – generating new XML from reusable templates and updating existing files by key.
- **Local Messages** – reading message definitions from Excel and exporting them into `.loc` files.

The solution is split into four projects to enforce a clear separation of concerns:

- `ToolBoxV2.Domain` – pure domain models (no infrastructure, no UI).
- `ToolBoxV2.Application` – use cases and service interfaces (business logic).
- `ToolBoxV2.Infrastructure` – implementations for Excel, XML and file export.
- `ToolBoxV2.Presentation.WPF` – WPF MVVM frontend, theming, snackbar notifications, and manual.

While ToolBoxV2 is actively used, **this repository is not a finished commercial product**.  
Instead, it serves as a **playground for practicing CLEAN architecture, dependency injection, testable application design, and layered structure** on a real-world scenario (XML automation + Excel processing + WPF UI).

---

## What ToolBoxV2 Solves

ToolBoxV2 was created to streamline two repetitive and error-prone workflows in automation projects:  
**working with large XML configuration files** and **managing Local Messages from Excel**.  
Both tools save time, reduce mistakes, and ensure predictable, structured outputs.

---

## XML Editor — Streamlining XML Template & Update Workflow

Working with XML manually is slow, inconsistent, and prone to human error—especially when dealing with  
templates, repeated structures, or configuration files that must follow strict formatting.

The XML Editor in ToolBoxV2 solves this by providing:

- **Template-based XML generation**  
  Define a reusable XML template once, then generate consistent output by replacing placeholders dynamically.

- **Structured XML updates using keys**  
  Instead of manually searching inside XML, the editor updates elements based on `XMLKeyDefinition`,  
  ensuring only the correct nodes are changed.

- **Readable, formatted output**  
  Generated XML is properly structured and indented using a custom formatter.

- **Integrated WPF editor**  
  The UI uses syntax highlighting, preview, and diagnostics to make XML manipulation clearer and safer.

**Result:**  
A reliable, repeatable workflow that replaces manual editing with deterministic generation and updates.

---

## Local Messages Generator — Simplifying .loc File Creation

Local Messages are often prepared in Excel and later transformed into `.loc` files.  
Doing this by hand or with inconsistent scripts can lead to:

- duplicated entries  
- missing messages  
- incorrect encoding  
- structural mistakes in `.loc` files  
- long development and edit time

The Local Messages Generator automates this process:

- **Universal Excel reader**  
  A header-based ClosedXML reader that adapts to any column order.

- **Reliable `.loc` export**  
  Uses a dedicated export service to build the correct internal structure each time.

- **Validation & diagnostics**  
  Snackbar notifications and console messages reveal warnings, errors, and generation status.

**Result:**  
Fast, consistent `.loc` file generation with significantly fewer mistakes and no manual post-processing.

---

## Architecture Overview

ToolBoxV2 is intentionally designed around **CLEAN Architecture** principles.  
The goal is to build a system where:

- **business logic is independent of UI and infrastructure**,  
- **each layer has a single responsibility**,  
- the codebase is **maintainable, testable, and scalable**.

### Domain (Core of the application)

The **Domain** contains pure business models such as:

- `LocalMessage`
- `XMLNodeModel`
- `XMLBlock`
- `XMLKeyDefinition`

This project has **no dependencies** — no file IO, no WPF, no external libraries.  
It represents the problem space, not the tools used to solve it.

---

### Application (Use cases + interfaces)

The **Application** layer defines what the system does:

- rules for generating Local Messages  
- interfaces for reading Excel  
- logic for XML template replacements  
- logic for XML updates  
- request → processing → result flows

Examples:

- `IExcelReader`
- `ILocalMessageExportService`
- `IXMLTemplateService`
- `IXMLNodeEditService`

This layer contains **interfaces only** (abstractions) and business orchestration.  
It contains *zero* implementations — making it easy to test and easy to replace.

---

### Infrastructure (Implementations & IO)

The **Infrastructure** layer provides real implementations:

- Excel reading (`ClosedXMLExcelReader`)
- XML reading and writing (`XMLReaderService`, `XMLExportService`, etc.)
- Local Messages export service
- Formatting and file creation helpers

This layer depends on external libraries:

- ClosedXML  

All implementations are registered through **Dependency Injection** and injected into Application.

---

### Presentation (WPF MVVM UI)

The **Presentation.WPF** project is the outermost shell:

- Views (XAML)
- ViewModels
- Commands
- Converters
- Snackbar system
- Diagnostic console
- Application services for file dialogs
- The included Manual.pdf

It depends only on the **Application** layer via interfaces.  
This ensures the UI can evolve independently from the core logic.

---

### Why this architecture?

By enforcing these boundaries:

- XML and Excel logic stays reusable  
- WPF UI stays clean and lightweight  
- Business rules can be unit tested independently  
- Infrastructure can be swapped or extended  
- Responsibilities are easy to understand and maintain  

ToolBoxV2 is both a functional tool and a practical example of **CLEAN architecture applied to a real problem**.

---

## Project Structure

ToolBoxV2 is organized into four projects that reflect the CLEAN Architecture layering.
ToolBoxV2/
│
├── ToolBoxV2.Domain/ # Domain models (pure business objects)
│
├── ToolBoxV2.Application/ # Use cases + service interfaces
│
├── ToolBoxV2.Infrastructure/ # Implementations (IO, XML, Excel, Logging)
│
└── ToolBoxV2.Presentation.WPF/ # WPF MVVM UI layer (frontend)

---

## Features at a Glance

ToolBoxV2 focuses on two major workflows — **XML editing** and **Local Message generation** — supported by a clean architecture and a modern WPF UI.

### XML Editor
- **Template-based XML generation**  
  Generate new XML files from reusable templates with placeholder bindings.
- **Key-based XML updates**  
  Modify existing XML documents using structured `XMLKeyDefinition` rules.
- **XML reading & preview**  
  Parse and preview XML inside the WPF editor with syntax highlighting.
- **Automatic formatting**  
  Outputs clean, indented, and readable XML structure.
- **Extensible services**  
  All XML operations are abstracted behind interfaces for testability.

---

### Local Messages Generator
- **Reliable `.loc` file export**  
  Generates valid Local Messages files using an isolated export service.
- **Diagnostics & warnings**  
  Detects missing fields, duplicates, or structural issues.

---

### Universal Excel Reader
- **Header-based mapping** (no fixed column positions)  
- Supports dynamic row structure and flexible sheet names  
- Reusable across XML Editor, Local Messages, and future tools  
- Fully injected via DI to keep UI and logic independent  

---

### WPF MVVM Frontend
- **Modern Material Design UI**  
  Custom themes, colors, fonts (Roboto, Noto), and DPI-aware layout.
- **Snackbar notification system**  
  - Queued messages  
  - Four levels (Success, Info, Warning, Error)  
  - Global access from any ViewModel  
- **Diagnostic console**  
  Structured logging visible directly in the app.
- **User-friendly views**  
  - XML editor  
  - Local Messages manager  
  - Initial dashboard  
- **Manual included** (`Docs/Manual.pdf`)

---

## 🧱 Tech Stack

ToolBoxV2 is built on a modern .NET ecosystem with a focus on clean separation, maintainability, and UI responsiveness.

### Core Technologies
- **.NET 9**
- **C#**
- **WPF (Windows Presentation Foundation)**
- **MVVM (Model–View–ViewModel)** architecture

---

### Architecture & Patterns
- **CLEAN Architecture**
- **Dependency Injection** (interface-driven design)
- **Separation of Concerns**
- **Testable Application Layer**
- **Domain-driven modeling** (simple, focused domain objects)
- **Service abstractions** for XML, Excel, and logging

---

### Infrastructure Libraries
- **ClosedXML**  
  Excel reading with header detection and dynamic column mapping.
- **Custom IO utilities**  
  - `StringWriterWithEncoding`
  - `.loc` file exporter
- **AvalonEdit**  
  Syntax-highlighted XML editor inside the WPF UI.

---

### WPF UI & Styling
- **Material Design in XAML Toolkit**  
  Modern UI components and styles.
- **Snackbar system**  
  Queued notifications with color-coded severity levels.
- **DPI-aware layout**  
  Fully resizable UI using Grids, dynamic sizing, and reusable styles.
- **Custom theming system**  
  This project includes a fully handcrafted theming layer built on top of WPF’s Resource Dictionary system.  
  All styles were created manually to ensure a consistent, modern, and scalable UI:

  - Centralized `Colors.xaml` with application-wide color definitions  
  - Custom **DataGrid styles** (headers, rows, mouse-over behavior, alternating rows)  
  - Custom **Button styles** (primary, secondary, icon buttons, menu buttons)  
  - Custom **TextBox / ComboBox styles** with focus animations and clean borders  
  - **Metrics.xaml** for spacing, margins, heights, and reusable layout guidelines  
  - Structure designed to support theme expansion in the future  

  These styles give ToolBoxV2 a polished, professional appearance and demonstrate a strong focus on UI/UX craftsmanship.

---

##  User Manual

A detailed **user manual** is included directly in the repository to help you understand and operate the application.

---

## Disclaimer, Limitations & Notes

### Disclaimer
ToolBoxV2 is an **educational and internal-use project**.  
It is not an official or commercial tool, and it is **not affiliated with any automation platform or vendor**.

The purpose of this repository is to:

- explore CLEAN Architecture in a real-world scenario  
- practice dependency injection and layered design  
- experiment with WPF, MVVM, XML generation, and Excel processing  
- provide a functional example of maintainable, testable code  

While the application is used actively, it is **not intended as a production-grade solution**.

---

### Limitations
ToolBoxV2 currently has inherent limitations due to its educational focus:

- The XML editor supports structured template-based workflows,  
  but it does **not** include a full schema validator.
- The Local Messages workflow is optimized for specific use cases  
  and **may not handle all edge-case `.loc` formats**.
- Excel import relies on ClosedXML; extremely large files may load slowly.
- The UI is WPF-only and runs on Windows systems.
- No database or cloud storage integration is included.
- Some workflows rely on manually prepared templates (XML or Excel).

These limitations are intentional — the goal is clarity, learning, and structure over feature breadth.



