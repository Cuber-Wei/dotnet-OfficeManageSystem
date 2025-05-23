## 项目简介
本项目是NUAA《现代软件开发技术实践》的实验内容。

## 具体要求
实现办公物品管理系统，系统具有两种类型的用户，管理员和普通用户。
- 管理员用户
    1. 采用桌面应用程序开发。
    2. 管理员登录功能：输入用户名、密码，需要符合userid=admin，password=0000进入系统主界面。用验证控件实现对用户名，密码不能为空的提醒。
    3. 系统主界面包括功能：用户维护，物品信息，物品入库，物品出库，库存查询；

用户维护：维护普通用户的信息，包括：用户ID、密码、姓名、性别、出生日期，联系电话。可以对用户进行增删改。

物品信息：维护物品基本信息，包括：物品编码、物品名称、物品类别（含纸张、文具、刀具、单据、礼品、其它。单选）、产地（下拉式列表框实现）、规格、型号、物品图片（附件上传），可以进行增删改；

物品入库：对物品入库信息进行管理，包括：入库流水号（自动生成），物品编码，购买日期、购买数量、单价、总价。保存后需要修改物品信息中的数量。

物品出库：查看普通用户的领用申请，确认，实现物品库存的扣减；

库存查询：查看物品的库存信息、入库明细、领用明细；

- 普通用户
    1. 采用Web应用开发。
    2. 登录功能：输入用户名、密码，需要符合管理员维护的用户名和密码进入系统主界面。需要验证实现对用户名，密码不能为空的提醒。
    3. 主界面包括两个功能库存查询、领用申请：

库存查询：查看物品库存信息

领用申请：提出物品领用申请，包括：领用流水（自动生成）、领用物品编码、数量、领用人（登录者）、领用日期、状态（申请、确认、驳回）（注意：领用物品不能直接输入编码，要能够选择一个物品，可以下拉选择，也可以弹出窗口选择）

实现要求：用户信息存在文件中（XML或JSON），其余信息存储在数据库中。

## 项目技术栈
管理员端采用`MAUI`框架，Web端采用`MVC`框架。

数据库采用`MySQL`，详细建表语句见`db.sql`；用户表见`user.json`。

数据库交互采用`Linq`。

开发所用系统：`Windows 10`

## TODO
1. 用户编辑页面新增电话号码校验
2. 添加用户头像字段
3. Web端添加退出登录功能
