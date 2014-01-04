artifactory-cleanup-client
==========================

.Net client to clean up (delete) old artifacts from the Artifactory repository
Requires .Net 4.5 Library and compatible JSON.net libraries

1.  Main screen has the login details. Enter your Artifactiry URL and login ID and Password.  If Login is successful, list of repositories is displayed.
2.  Select the date criteria to search for repositories and choose Run.
3.  List of artifacts is now displayed.  Select and Delete whatever is not needed.
4.  Deleting artifacts can leave behind empty folders.  In the main screen, select the repository and choose Delete Empty Folders to delete all empty folders in the selected repository.

Note: I have tested against my Artifactory version 2.6.7.  Please test with your repository version on test folders before using it productively.