AddEntryApp

- Purpose: Simple console app that appends an English entry with an empty Finnish translation to the workspace `translations.csv`.
- Run:

```powershell
cd AddEntryApp
dotnet run
```

- Usage: Type the English text and press Enter. The app appends a CSV row `"English",""` to `translations.csv` and waits for the next input. Leave input empty and press Enter to quit.
