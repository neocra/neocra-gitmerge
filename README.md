# Neocra GitMerge

This project is to solve merge with the semantic of the file.

Example :
- Ancestor
```xml
<root>
</root>
```
- master branche
```xml
<root>
    <node1 />
</root>
```
- feature branche
```xml
<root>
    <node2 />
</root>
```
With default git merge this file is in conflict. With this driver the result is :
```xml
<root>
    <node1 />
    <node2 />
</root>
```

## How to Install this git driver 

