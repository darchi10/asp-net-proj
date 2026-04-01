$data = [Console]::In.ReadToEnd()

Add-Content -Path "C:\Users\Dario\Desktop\faks\6.semestar\ASP.NET\projekt\.github\hooks\agent_log.txt" -Value "`n--- LOG ENTRY ---`n"
Add-Content -Path "C:\Users\Dario\Desktop\faks\6.semestar\ASP.NET\projekt\.github\hooks\agent_log.txt" -Value $data