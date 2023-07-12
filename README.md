# CloudGarage

## 指令
`/cg s <Name>`将当前目视的载具以`<Name>`的名称存入SQL数据库（云车库）  
`/cg d <Name>`取出SQL数据库（云车库）中名为`<Name>`的载具（只能取出本人存放的载具）  
`/cg l`列出SQL数据库（云车库）中执行者（玩家）所拥有的载具  
### 注意
不同玩家的载具可重名；但若一个玩家尝试存入两辆同名载具，插件会拒绝存入并提示存入失败。  
该插件需要[MySQL Connector/NET 8.0.33](https://dev.mysql.com/downloads/connector/net/8.0.html)或以上版本！请下载安装之后将安装路径下的`MySQL.Data.dll`复制到`Rocket\Libraries\`下

## 配置文件
默认配置文件：
```xml
<?xml version="1.0" encoding="utf-8"?>
<CloudGarageConfig xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <SQLAddress>127.0.0.1</SQLAddress>
  <SQLPort>3306</SQLPort>
  <SQLUsername>root</SQLUsername>
  <SQLPassword>password</SQLPassword>
  <DatabaseName>unturned</DatabaseName>
  <DatabaseTableName>cloudgarage</DatabaseTableName>
</CloudGarageConfig>
```
装入插件第一次启动服务器后，自行配置该配置文件以确保其能正常，并在目标SQL服务器上创建好数据库（数据表无须也不建议手动创建）
