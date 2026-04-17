# Changelog

All notable changes to AcademicAI Suite will be documented in this file.

## [3.0.0] - 2026-04-16

### Added
- 7-section hub navigation: Dashboard, Study Hub, Planner, Research, Writer, Text Tools, Settings
- **Study Hub** with tabbed sub-tools: Flashcards, Quiz Generator, Note Organizer, Math Solver
- **Research** with tabs: Research Library, Citations
- **Writer** with tabs: Academic Writer, Essay Outliner, Grammar Checker
- **Text Tools** with tabs: Summarizer, Paraphraser, Humanizer, Translator, AI Detector
- Flashcard generator with difficulty selector (Easy/Medium/Hard)
- Quiz generator with multiple quiz types (Multiple Choice, True/False, Short Answer, Mixed)
- Essay outliner with essay types and academic levels
- Paraphraser with style options (Academic, Casual, Creative, Concise)
- Grammar checker with error detection and corrections
- Math solver with step-by-step solutions
- Note organizer for structuring raw notes
- Reduced AI providers to 2: OpenRouter and Fireworks
- Kill switch via remote control.json (global deactivation, license revocation)
- Auto-update system with mandatory update support
- AES-256 encryption for API keys (derived from machine identity)
- 3-step onboarding wizard with Next/Back navigation
- 5-language support: English, Arabic (RTL), French, Spanish, German
- Windows 11 Fluent Design with Mica backdrop
- System tray with context menu
- Floating Pomodoro timer window
- PDF drag-and-drop text extraction
- Markdown viewer with theme-aware rendering
- Cancel buttons on all AI operations
- Copy result buttons on all tools

### Changed
- Consolidated 18 navigation items into 7 sections with tabbed sub-pages
- AES encryption key now derived from machine identity instead of hardcoded value
- Removed 7 AI providers (Groq, DeepSeek, Cerebras, Mistral, Gemini, Claude, Cloudflare)
- Replaced ContentControl with Frame for proper Page hosting in hub views

### Security
- Fixed hardcoded AES key/IV in source code (now machine-derived)
- API leak audit completed — no secrets in repository
