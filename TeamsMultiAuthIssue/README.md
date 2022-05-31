# Overview
This is a barebones project which contains the minimal amount of code for authenticating a Teams App built-in C# .NET Framework. Within this code contains a Microsoft's Silent Authentication using ADAL (which Microsoft will be depricate in mid 2022) as well as the built-in authenicate code from the base MVC template.

# Purpose
The purpose of this project is for troubleshooting an issue where an individual logs into the app under different logins. (e.g., Their standard account, an account for demo purposes, an admin account, etc.). When switching accounts, the user will encounter a blank white screen immediately after Authentication.

# Steps to Reproduce
In order to best demo this behavior, it's best to clear your Teams cache and disconnect any Work And School settings. However, in most cases that this occurs, the individual will be connected to their company's domain.

# Initial Setup
## Start NGrok
Get NGrok forwarding subdomain (https://«ngrok_forwarding_subdomain».ngrok.io)

## Create new App Registration

1. https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade
2. Click New registration
3. Enter Name: TestTeamsAuthSSO
4. Select "Accounts in any organizational directory (Any Azure AD directory-Multitenant)
5. Select Web in Redirect URI: 
6. Put in the https://«ngrok_forwarding_subdomain».ngrok.io/auth/msteamsSilentEnd
	e.g., https:/0000-0-00-000-000.ngrok.io/auth/msteamsSilentEnd
7. Click Register
8. Click Authentication in sidebar
9. Add another redirect to the https://«ngrok_forwarding_subdomain».ngrok.io
        e.g., https:/0000-0-00-000-000.ngrok.io
10. Turn Access tokens and ID tokens
11. Click Save
12. Click Overview in sidebar
13. Copy the Application (client) ID

## Update Web.config
1. Update the ClientId with the Application (client) ID from the App Registration
2. Update the redirect Uri with the https://«ngrok_forwarding_subdomain».ngrok.io from the ngrok

## Setup Manifest
1. Add a new Id 
2. Replace the contentUrl/WebsiteUrl with the ngrok forwarding url
      "contentUrl": "https://«ngrok_forwarding_subdomain».ngrok.io/Home/Index",
      "websiteUrl": "https://«ngrok_forwarding_subdomain».ngrok.io/Index",
3. Create zip file containing Manifest, icon-color.png and icon-outline.png

## Add the new app to teams
Sideload manifest zip to teams

Make sure there are no other Work and School
