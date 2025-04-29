-- 创建库
create database if not exists dot_net_project_db;

-- 切换库
use dot_net_project_db;

-- 物品表
create table if not exists item
(
    id          bigint auto_increment comment 'id' primary key comment '物品id',
    code        varchar(32)                        not null comment '编码',
    itemName    varchar(256)                       not null comment '名称',
    itemType    int      default 0                 not null comment '类型（0-纸张/1-文具/2-刀具/3-单据/4-礼品/5-其他）',
    origin      varchar(64)                        null comment '产地',
    itemSize    varchar(64)                        null comment '规格',
    itemVersion varchar(64)                        null comment '型号',
    itemPic     varchar(1024)                      null comment '物品图片',
    itemNum     int      default 0                 not null comment '库存数量',
    createTime  datetime default CURRENT_TIMESTAMP not null comment '创建时间',
    updateTime  datetime default CURRENT_TIMESTAMP not null on update CURRENT_TIMESTAMP comment '更新时间',
    isDelete    tinyint  default 0                 not null comment '是否删除',
    index idx_code (code)
) comment '物品' collate = utf8mb4_unicode_ci;

-- 入库表
create table if not exists import_record
(
    id          bigint auto_increment comment 'id' primary key comment '入库流水',
    itemId      bigint                             not null comment '物品 id',
    importNum   int      default 0                 not null comment '购买数量',
    singlePrice decimal(10, 2) default 0.00        not null comment '单价',
    importDate  datetime default CURRENT_TIMESTAMP not null comment '购买日期',
    createTime  datetime default CURRENT_TIMESTAMP not null comment '创建时间',
    updateTime  datetime default CURRENT_TIMESTAMP not null on update CURRENT_TIMESTAMP comment '更新时间',
    isDelete    tinyint  default 0                 not null comment '是否删除',
    index idx_itemId (itemId)
) comment '领用申请' collate = utf8mb4_unicode_ci;

-- 领用申请表
create table if not exists apply_record
(
    id          bigint auto_increment comment 'id' primary key comment '领用流水',
    userId      bigint                             not null comment '申请人 id',
    itemId      bigint                             not null comment '物品 id',
    applyNum    int      default 0                 not null comment '领用数量',
    applyStatus int      default 0                 not null comment '申请状态（0-申请/1-确认/2-驳回）',
    applyDate   datetime default CURRENT_TIMESTAMP not null comment '领用日期',
    createTime  datetime default CURRENT_TIMESTAMP not null comment '创建时间',
    updateTime  datetime default CURRENT_TIMESTAMP not null on update CURRENT_TIMESTAMP comment '更新时间',
    isDelete    tinyint  default 0                 not null comment '是否删除',
    index idx_itemId (itemId),
    index idx_userId (userId)
) comment '领用申请' collate = utf8mb4_unicode_ci;