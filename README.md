# SharedLibrary
.Net 4.6.1 share library that I use in almost every project.
<strong>Some code could be found in other projects, taken from everywhere, modified or written myself.  </strong>

It is fully tested and helps me get up and running quickly. I have moved it recently from bitbucket to Git.  I try to update it 
everytime I come across a new challenge.  I have built this over years and I hope it helps others too.

<h1>Approach</h1>
<P>Git flow </p>
<P>TDD with BDD wording - So start from the tests</p>
<P>Full Integration tests - Database needed</p>
<P>Dependency inversion principle - Interfaces and implementation will be in seperate projects.  They are known as contracts in this instance.</p>

<h1>Setup</h1>
Pull, build, nuget should add all packages.  To run tests locally it will attempt to create a DB on your localhost and most tests with the exception of the 
stored procedure tests will pass.  

<p>Recommended setup however would be to use the database project to update your local testing database then to run the tests. </p>

<h1>Sections</h1>
<h2>Controllers</h2>
<p>Odata - Full http verbs on entities</p>
<p>Odata read only</p>
<p>Odata composite - Odata controller for entities with a composite primary key</p>

<h2>Logging</h2>
<p>Logging wrapper for NLog</p>
<p>Ability to easily set level</p>
<p>Ability to add custom properties</p>

<h2>Repository</h2>
<p>Database factory</p>
<p>Base repository - CRUD, stored procedures etc. </p>
<p>Unit of work</p>

<h2>Helper</h2>
Any static helper classes that I may feel would be useful in other projects.  

<h2>Test helper</h2>
The most important section in my opinion.  

<p>Database mocker</p>
<p>Equality helper - checks all properties of objects and compares them for testing purposes while ignoring some</p>
<p>File helper - to retrieve relative checked in files for testing</p>
<p>Rollback - Wraps tests in an transaction to never alter the database</p>
<p>etc,</p>
