# CloudGarage

## ָ��
`/cg s <Name>`����ǰĿ�ӵ��ؾ���`<Name>`�����ƴ���SQL���ݿ⣨�Ƴ��⣩  
`/cg d <Name>`ȡ��SQL���ݿ⣨�Ƴ��⣩����Ϊ`<Name>`���ؾߣ�ֻ��ȡ�����˴�ŵ��ؾߣ�  
`/cg l`�г�SQL���ݿ⣨�Ƴ��⣩��ִ���ߣ���ң���ӵ�е��ؾ�  
### ע��
��ͬ��ҵ��ؾ߿�����������һ����ҳ��Դ�������ͬ���ؾߣ������ܾ����벢��ʾ����ʧ�ܡ�  
�ò����Ҫ[MySQL Connector/NET 8.0.33](https://dev.mysql.com/downloads/connector/net/8.0.html)�����ϰ汾�������ذ�װ֮�󽫰�װ·���µ�`MySQL.Data.dll`���Ƶ�`Rocket\Libraries\`��

## �����ļ�
Ĭ�������ļ���
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
װ������һ���������������������ø������ļ���ȷ����������������Ŀ��SQL�������ϴ��������ݿ⣨���ݱ�����Ҳ�������ֶ�������
