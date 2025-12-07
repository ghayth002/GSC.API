$baseUrl = "http://localhost:5114"
$adminEmail = "ghaythweslaty10@gmail.com"
$adminPassword = "Password123!" # Assuming this is the password or similar. 
# Wait, I don't know the admin password. The user provided email credentials but not login password.
# The summary said "Email Sender Credentials...".
# I'll try to register a new admin or use a known one?
# I'll try to Register a new user first to be sure.

# Actually, I can just check the database context if I could, but I can't.
# I'll try to use the "Fournisseur" creation flow which generates a password.
# But I need to be Admin to create a Fournisseur.

# Let's assume the user has a standard admin account.
# If I can't login, I can't test.

# Alternative: I can add a temporary "Backdoor" or logging in the controller to dump the created menu to the console?
# That's easier.

# I'll add Console.WriteLine in AccepterDemande to print the created menu details.
# And Console.WriteLine in GetAvailableMenusForVol to print what it's returning.
# Then I'll ask the user to try again and I'll check the logs.

# This is safer than guessing passwords.
