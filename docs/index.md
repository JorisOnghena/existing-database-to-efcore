# Existing database to efcore
A small Windows forms application to convert tables of an existing database to entity framework core classes.
The idea of the project is to be be able to

 - Create c# (or really any code) from an existing table.
 - Be able to plug in new databases and code generation

## Screenshots
A few screenshots of the program.

*Main screen*

[![Main](https://raw.github.com/ShiveringSquirrel/existing-database-to-efcore/master/screenshots/main_small.png "Main")](https://raw.github.com/ShiveringSquirrel/existing-database-to-efcore/master/screenshots/main.png)

*Create/edit configuration*

[![Connection](https://raw.github.com/ShiveringSquirrel/existing-database-to-efcore/master/screenshots/connection.png "Connection")](https://raw.github.com/ShiveringSquirrel/existing-database-to-efcore/master/screenshots/connection.png)

## Example configuration file
A sample ini config file used by this program.
~~~~
[Connection]
DisplayName = Test
String = Your connection string
Type = MySQL
~~~~

<sub>Icon made by <a href="https://www.flaticon.com/authors/pixel-perfect" title="Pixel perfect">Pixel perfect</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a> is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0" target="_blank">CC 3.0 BY</a><sub>