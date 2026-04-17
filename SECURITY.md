# Security Policy

## Supported Versions

| Version | Supported |
| ------- | --------- |
| 3.0.x   | ✅ |
| < 3.0   | ❌ |

## Reporting a Vulnerability

**Do NOT open a public GitHub issue for security vulnerabilities.**

Instead, please report security issues by:

1. **Email**: Send details to the repository owner via [GitHub's private vulnerability reporting](https://github.com/MrBlizo/AcademicAI-Suite/security/advisories/new)
2. **GitHub Security Advisory**: Use the "Report a vulnerability" button on the Security tab

Please include:
- Description of the vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (if any)

## Response Time

- Acknowledgment within 48 hours
- Initial assessment within 7 days
- Fix timeline communicated within 14 days

## Known Security Measures

- API keys are encrypted with AES-256 using machine-derived keys
- No API keys or secrets are stored in source code
- Kill switch enables remote deactivation if a critical vulnerability is discovered
- License revocation available for compromised keys
