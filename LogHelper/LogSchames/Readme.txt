安装elasticSearch服务：
1、安装JDK；
2、到elasticSearch的/bin目录下打开cmd，运行service.bat install,注册服务；
3、进入windows服务，启动elasticsearch服务进程；
4、访问localhost:9200查看服务启动情况；
5、注册两个mapping。进入script目录，底下有两个文件：biz_schema.json、Program_Schema.json；在当前目录启动CMD；运行
curl -XPOST http://localhost:9200/log -d @Program_Schema.json
curl -XPOST http://localhost:9200/log -d @biz_schema.json

两条命令；如果没有错误，即表示成功。注：运行CURL之前，必须在%path%环境变量中填入curl.exe的地址。