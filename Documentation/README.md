# 📚 Project Documentation

Centralizirana dokumentacija za **Mobile Phone Service and Sales System** projekt.

---

## 📑 Pregled Dokumentacija

### 🎯 Kriteriji Ocjenjivanja (7/8 Implementirano)

| # | Kriterij | Bodovi | Dokumentacija |
|---|----------|--------|---------------|
| 1 | **CRUD operacije** | 2 | [`ARCHITECTURE.md`](#architecture) |
| 2 | **MCP Server** | 2 | [`MCP_IMPLEMENTATION.md`](#mcp-server), [`MCP_README.md`](#mcp-server) |
| 3 | **Logging** | 2 | [`LOGGING_IMPLEMENTATION.md`](#logging), [`LOGGING_README.md`](#logging), [`LOGGING_QUICKSTART.md`](#logging) |
| 4 | **Responsive UI** | 2 | [`RESPONSIVE_IMPLEMENTATION_SUMMARY.md`](#responsive-ui), [`RESPONSIVE_IMPLEMENTATION.md`](#responsive-ui), [`RESPONSIVE_QUICK_GUIDE.md`](#responsive-ui) |
| 5 | **AI Integracija** | 3 | [`AI_INTEGRATION.md`](#ai-integration) |
| 6 | **Global Search** | 2 | [`GLOBAL_SEARCH.md`](#global-search) |
| 7 | **Integration Tests** | 2 | [`TESTING.md`](#testing) |
| 8 | **Cloud Deploy** | 3 | ❌ Nije implementirano |

**Total:** 15/18 bodova (83.3%)

---

## 📖 Dokumenti po Kategorijama

### 🏗️ Architecture

#### [`ARCHITECTURE.md`](ARCHITECTURE.md) (1033 linije)
**Tema:** Project Architecture & CRUD Operations

**Sadržaj:**
- High-level arhitektura (4 layers: Presentation, Business, Data, Database)
- Project structure tree
- MVC pattern implementation
- Svih 7 domain entities (Product, Customer, Phone, RepairJob, Order, SparePart, Technician)
- Standard CRUD controller pattern
- Soft delete pattern
- DTO mapping strategy
- Validation strategy
- Authorization strategy
- Error handling
- Database context (EF Core)
- Identity & Authentication
- Performance considerations
- Best practices

**Koristi za:**
- Razumijevanje arhitekture projekta
- CRUD pattern implementation
- Best practices i conventions

---

### 🔍 Global Search

#### [`GLOBAL_SEARCH.md`](GLOBAL_SEARCH.md) (736 linija)
**Tema:** Global Search Functionality

**Sadržaj:**
- Arhitektura search funkcionalnosti
- Search algorithm (4 faze)
- Role-based access control (RBAC tablica)
- Customer linking logic
- Database entity search za 7 entiteta
- SearchResultDto struktura
- 4 API usage primjera
- Integration tests
- Performance optimizations
- Security measures
- Frontend integracija

**Koristi za:**
- Razumijevanje global search implementacije
- Role-based filtering logika
- API usage primjeri

---

### 🧪 Testing

#### [`TESTING.md`](TESTING.md) (737 linija)
**Tema:** Integration Testing Documentation

**Sadržaj:**
- Test coverage (64 tests, 8 files, 100% API coverage)
- Test architecture (TestWebApplicationFactory, TestAuthHandler)
- Technology stack (xUnit, FluentAssertions, InMemory DB)
- Test pattern (Arrange-Act-Assert)
- 6 detaljnih test primjera
- Test coverage po entitetu
- Kako pokrenuti testove
- Best practices
- Template za nove testove

**Koristi za:**
- Razumijevanje test arhitekture
- Pisanje novih testova
- Pokretanje testova

---

### 🤖 MCP Server

#### [`MCP_IMPLEMENTATION.md`](MCP_IMPLEMENTATION.md) (742 linije)
**Tema:** Model Context Protocol - Detailed Implementation

**Sadržaj:**
- MCP arhitektura i komponente
- Implementation details (NuGet, Program.cs, Tool pattern)
- OAuth metadata endpoint objašnjenje
- Svih 21 MCP tool-ova dokumentirano
- Setup & configuration guide
- 3 kompleksna usage primjera
- Troubleshooting guide
- Technical details (protocol specs, security, performance)

**Koristi za:**
- Deep dive u MCP implementaciju
- Troubleshooting connection issues
- Understanding OAuth discovery

#### [`MCP_README.md`](MCP_README.md) (424 linije)
**Tema:** Model Context Protocol - User Guide

**Sadržaj:**
- Pregled MCP protokola
- Popis svih 21 MCP tool-ova s parametrima
- Connection examples za Claude Desktop, Cursor, VS Code Copilot
- JSON request/response primjeri
- Security best practices

**Koristi za:**
- Brzi pregled MCP tool-ova
- Connection setup za različite klijente
- API reference

---

### 📝 Logging

#### [`LOGGING_IMPLEMENTATION.md`](LOGGING_IMPLEMENTATION.md)
**Tema:** Logging Mechanism - Implementation Summary

**Sadržaj:**
- Serilog framework konfiguracija
- Automatsko HTTP request logging
- CrudActionLoggingFilter
- UnhandledExceptionLoggingMiddleware
- Log levels i output
- Dokumentacija linkovi

**Koristi za:**
- Pregled logging implementacije
- Log file locations
- Quick reference

#### [`LOGGING_README.md`](LOGGING_README.md)
**Tema:** Logging Mechanism - Detailed Documentation

**Sadržaj:**
- Potpuna Serilog integracija dokumentacija
- Konfiguracija po environmentu
- Primjeri korištenja ILogger
- Structured logging
- Log file rotation
- Best practices

**Koristi za:**
- Detailed logging reference
- How to add logging to new controllers

#### [`LOGGING_QUICKSTART.md`](LOGGING_QUICKSTART.md)
**Tema:** Logging Mechanism - Quick Start Guide

**Sadržaj:**
- Brzi vodič za developere
- Kako dodati logging
- Log level primjeri
- Troubleshooting

**Koristi za:**
- Quick start za nove developere
- Common issues

---

### 📱 Responsive UI

#### [`RESPONSIVE_IMPLEMENTATION_SUMMARY.md`](RESPONSIVE_IMPLEMENTATION_SUMMARY.md)
**Tema:** Responsive Mobile/Web UI - Summary

**Sadržaj:**
- Implementation summary
- CSS responsive enhancements (~330 linija)
- JavaScript mobile optimizations (~130 linija)
- Layout updates
- Testing checklist

**Koristi za:**
- Quick overview responsive features
- Testing different devices

#### [`RESPONSIVE_IMPLEMENTATION.md`](RESPONSIVE_IMPLEMENTATION.md)
**Tema:** Responsive Mobile/Web UI - Detailed

**Sadržaj:**
- Detaljnija implementacija
- Media query breakdown
- Touch event handling
- iOS Safari viewport fix
- Performance optimizacije

**Koristi za:**
- Deep dive u responsive implementation
- Understanding CSS/JS changes

#### [`RESPONSIVE_QUICK_GUIDE.md`](RESPONSIVE_QUICK_GUIDE.md)
**Tema:** Responsive Mobile/Web UI - Quick Guide

**Sadržaj:**
- Brzi vodič
- Kako testirati responsive design
- Common issues
- Browser compatibility

**Koristi za:**
- Quick testing guide
- Troubleshooting

---

### 🤖 AI Integration

#### [`AI_INTEGRATION.md`](AI_INTEGRATION.md)
**Tema:** AI Integration - Groq Implementation

**Sadržaj:**
- GroqAiService implementacija
- Products AI Parser endpoint
- Phones AI Parser endpoint
- Natural language parsing primjeri
- Groq API konfiguracija
- User secrets setup
- Error handling

**Koristi za:**
- Understanding AI integration
- How to use AI parse endpoints
- Natural language examples

---

### 📊 Summary

#### [`DOCUMENTATION_SUMMARY.md`](DOCUMENTATION_SUMMARY.md) (314 linija)
**Tema:** Documentation Overview & Mapping

**Sadržaj:**
- Mapa: Kriterij → Dokumentacija
- Detaljni pregled svih dokumentacija
- Ocjene kvalitete po kategoriji
- Scoring za dokumentaciju
- Preporuke za obranu projekta

**Koristi za:**
- Finding specific documentation
- Documentation quality assessment
- Preparation for project defense

---

## 📈 Statistika

- **Total Dokumentacija:** 12 dokumenata
- **Total Linija:** 3,506+ linija
- **Implementirani Kriteriji:** 7/8 (87.5%)
- **Dokumentirani Kriteriji:** 7/7 (100%)
- **Kvaliteta:** ⭐⭐⭐⭐⭐ Izvrsna

### Breakdown:
- **Architecture**: 1 dokument (1033 linije)
- **Global Search**: 1 dokument (736 linija)
- **Testing**: 1 dokument (737 linija)
- **MCP Server**: 2 dokumenta (1166 linija)
- **Logging**: 3 dokumenta (~600 linija)
- **Responsive UI**: 3 dokumenta (~500 linija)
- **AI Integration**: 1 dokument (~300 linija)

---

## 🎯 Preporuke za Čitanje

### Za **Obranu Projekta:**
1. Pročitaj [`DOCUMENTATION_SUMMARY.md`](DOCUMENTATION_SUMMARY.md) - pregled svega
2. Pročitaj [`ARCHITECTURE.md`](ARCHITECTURE.md) - razumijevanje arhitekture
3. Prelistaj specifične dokumentacije ovisno o pitanju

### Za **Development:**
1. [`ARCHITECTURE.md`](ARCHITECTURE.md) - CRUD patterns i conventions
2. [`TESTING.md`](TESTING.md) - kako pisati testove
3. Relevantnu dokumentaciju za feature na kojem radiš

### Za **Troubleshooting:**
1. [`MCP_IMPLEMENTATION.md`](MCP_IMPLEMENTATION.md) - MCP connection issues
2. [`LOGGING_QUICKSTART.md`](LOGGING_QUICKSTART.md) - logging issues
3. [`RESPONSIVE_QUICK_GUIDE.md`](RESPONSIVE_QUICK_GUIDE.md) - UI issues

---

## 🔗 Dodatni Resursi

### U Projektu:
- **Lab dokumentacija:** `../lab-1/`, `../lab-2/`, `../lab-3/`, `../lab-4/`, `../lab-5/`
- **Grading criteria:** `../final/Ocjenjivanje-seminar.md`
- **Progress summary:** `../current_progress/project_readiness_summary.md`

### External:
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)

---

## 📝 Izmjene

| Datum | Izmjena |
|-------|---------|
| 2026-07-01 | Kreiran Documentation folder, dodane GLOBAL_SEARCH.md, TESTING.md, ARCHITECTURE.md |
| 2026-07-01 | Dodana MCP_IMPLEMENTATION.md |
| 2026-06-25 | Dodana Responsive UI dokumentacija |
| 2026-06-24 | Dodana AI Integration i Logging dokumentacija |

---

**Projekt:** Mobile Phone Service and Sales System  
**Framework:** ASP.NET Core 10.0  
**Database:** MySQL 8.0  
**Author:** Dario (AI-Assisted)
