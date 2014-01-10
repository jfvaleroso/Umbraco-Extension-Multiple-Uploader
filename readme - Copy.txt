Step 1. Add Renaissance.CustomDataTypes.dll and Renaissance.CustomDataTypes.pdb to bin directory.
Step 2. Add multiple uploader as new data type.
Step 3. From the data type setting, set the Repository folder, set the max size, and allowable file extensions.
Step 3. add "guid" property in member property.
Step 4. add the following in umbraco > config > lang > en.xml
asdf

<area alias="relatedlinks">
    <key alias="addExternal">Add external link</key>
    <key alias="addInternal">Add internal link</key>
    <key alias="addlink">Add</key>
    <key alias="caption">Caption</key>
    <key alias="internalPage">Internal page</key>
    <key alias="linkurl">URL</key>
    <key alias="modeDown">Move Down</key>
    <key alias="modeUp">Move Up</key>
    <key alias="newWindow">Open in new window</key>
    <key alias="removeLink">Remove link</key>
    <key alias="modeDownload">Download</key>  
    <key alias="addInternalFile">Add Internal File</key>  
    <key alias="addExternalFile">Add External File</key> 
    <key alias="file">file</key>
 </area>sadf