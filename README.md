# Veeam Log Anonymizer 
[![Twitter: JMousqueton](https://img.shields.io/twitter/follow/JMousqueton.svg?style=social)](https://twitter.com/JMousqueton)

> __A tool to anonimyze your Veeam logs__


VeeamLogAnonymizeris is a C# program designed for anonymizing log files generated by [Veeam](https://www.veeam.com) Backup & Replication v12,  which may contain sensitive information like server names, IP addresses, email addresses, and more. Anonymizing log files helps protect privacy and confidentiality when sharing or analyzing these logs.

I want to clarify that I'm not a developer by profession, but rather a member of the Veeam community who saw the need for a tool like this. The script has been created out of a passion for data privacy and a desire to contribute to our community.

## ⚠️ Disclamer 

This script is still as an early stage of development. 

⚠️ UNDER CONSTRUCTION ⚠️

I want to make it clear that I am not employed by or associated with Veeam Software in any official capacity. This script, its development, and its distribution are entirely independent efforts. It is not endorsed or supported by Veeam Software.

The script has been created by a member of the Veeam community out of a genuine interest in data privacy and the needs of fellow users. While it is designed to work with Veeam Backup & Replication logs, it is not an official Veeam product, and any issues or concerns related to its use should be directed to me rather than Veeam Software.

Please use this script responsibly and in accordance with your organization's policies and any relevant data privacy regulations. Always ensure that you have proper backups and safeguards in place when making changes to log files.

Thank you for understanding the independent nature of this project.

## ⚙️ Features

Veeam Log Anonymizer, anonymizes : 

- Veeam Server name 
- Usernames 
- IPs address 
- SMTP Servers     
- vCenter Username 
- Domains
- vCenter Servers
- Email
- vCenter Location

## 💿 Installation

coming soon

## 🚀 Usage 

Veeam Backup logs Anonymizer C# edition v0.1

```
Usage :
-s <source dir> : absolute path to source dir containing Veeam logs.
-d <destination dir> : absolute path to destination dir where anonymized logs will be written. Must exists beforehand.
[-c <config file>] : absolute path to Json configuration file. If not present, will use config.json in the same dir as this executable.
```

## 📝 Examples 

```
.\vbrLogAnon.exe -s C:\log -d C:\Ano\ -c 'C:\Users\JMOUSQU\vbrLogAnon\config.json'
Source directory : C:\log
Destination directory : C:\Ano\
Multicore enabled. Possible high memory usage
Maximum in-memory file size: 25MB
Scanning source directory structure and building target directory structure
Target directory structure built.
Parsing files
......................................................................................................................................
Done processing 133 files in 6465ms
Dictionnary written at C:\log\VBRLogDic.txt
Exiting now. Goodbye.
```



## Authors

👤 **Eric Machabert**

* LinkedIn [Eric Machabert](https://www.linkedin.com/in/eric-machabert-5069b616)

👤 **Julien Mousqueton**

* Website: <https://www.julien.io>
* Twitter: [@JMousqueton](https://twitter.com/JMousqueton)
* Github: [@JMousqueton](https://github.com/JMousqueton)
* LinkedIn: [Julien Mousqueton](https://linkedin.com/in/julienmousqueton)

## 🤝 Contributing

Contributions, issues and feature requests are welcome!

Feel free to check [issues page](https://github.com/JMousqueton/Badware/issues).

## 🙏🏻 Acknowledgements

For the original idea, the help for developping in C# and the support and the ideas of improvement :   

* Bertrand (Veeam Legend)
* Eric (Veeam Vanguard)
  
## Show your support

Give a ⭐️ if this project helped you!

## 📝 License
