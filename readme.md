![pier](http://images.theleagueofpaul.com/pier.png)

Pier is a self hosted url shortening service designed to run on AppHarbor

## Installation instructions
Sign up for AppHarbor, create a new app, add a MSSQL database (addon, 20mb is fine) and name it `PierContext`

	git clone git@github.com:aeoth/Pier.git
	git remote add apphb <YourAppHbUrl>
	git push apphb master

And you're done.