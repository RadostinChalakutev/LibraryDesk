# LibraryDesk User Guide

## First launch and sign-in

On first launch, create the initial administrator account. Choose a password with at least 12 characters that includes uppercase, lowercase, and numeric characters. LibraryDesk has no default usernames or passwords.

Sign in with a local staff account. Administrators see all modules, including Settings. Librarians see the day-to-day operational modules but not account administration.

## Core circulation workflow

1. Open **Readers** and create a reader with a unique membership number.
2. Open **Books** and register the book title, then select it and choose **Generate barcode** before adding each physical copy. The generated barcode can be scanned or entered in Loans.
3. Open **Loans**, enter the reader membership number, copy barcode, and due date in `YYYY-MM-DD` format, then select **Borrow**.
4. Select an active loan and choose **Return selected** when the copy is received. The copy becomes available immediately and the loan remains in history.

Borrowing is blocked for inactive readers, unavailable copies, invalid dates, and readers that have an existing overdue loan.

## Daily operations

- **Dashboard** provides current reader, catalogue, available-copy, active-loan, and overdue counts.
- **Overdue** lists loans whose due date has passed and calculates the days late.
- **Reports** displays loan history for an inclusive borrowed-date range directly in the application. CSV export to Documents remains available for Excel or another spreadsheet program.

## Administration and backup

Administrators can create librarian or administrator accounts in **Settings**, activate/deactivate staff accounts, and create a point-in-time database backup. The signed-in administrator cannot deactivate their own account, and the final active administrator cannot be deactivated.

Backups are saved in `%UserProfile%\\Documents\\LibraryDesk\\backups`. Keep a copy somewhere protected and test your organization’s recovery process before relying on it in an emergency. LibraryDesk currently creates backups; it does not offer an in-application restore workflow.

## Data locations

- Database: `%LocalAppData%\\LibraryDesk\\LibraryDesk.db`
- Logs: `%LocalAppData%\\LibraryDesk\\logs`
- Exports: `%UserProfile%\\Documents\\LibraryDesk\\reports`
- Backups: `%UserProfile%\\Documents\\LibraryDesk\\backups`
