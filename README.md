# FeatherSQL简介

FeatherSQL是众多ORM轮子中的一个，我开发它的初衷是因为公司的某个项目中使用了Dapper作为了数据访问层（没有扩展的原生Dapper），虽然性能很高，却要开发者完全拼写SQL语句，哪怕是简单的增删改查，于是我就准备做一个Dapper的扩展，写了一半的时候被项目经理告知Dapper的扩展有很多，我去网上查了一下，嘿，还真有很多。

在Nuget上搜索Dapper排名比较靠前的有：

* Dapper.Extension
* Dapper.Fluent
* Dapper.SimpleCRUD
那我为什么还要造轮子？原因很简单，**我想把这些大佬的优点集合在一个包里**，索性不依赖Dapper造起了轮子，也就有了现在的FeatherSQL。

# 优、缺点

## 缺点

支持的数据库有限，由于鄙人能力和精力有限，这个包暂时仅支持**SqlServer2012**以上版本数据库。

性能，事实上FeatherSQL的执行速度并不快，但要优于EntityFramework。

## 优点

良好的文档，我花了很长时间写了这份文档，把绝大多数应用场景都描述到了，并且附有源码。

丰富的接口，这是我写这个包的主要目的，它考虑到了绝大多数的CRUD场景，并且提供了简单的调用方式。

# 开始使用

## 安装

Nuget安装：

在Nuget包管理中搜索FeatherSQL,点击安装，或在Package Manager执行以下命令：

```
Install-Package FeatherSQL -Version 1.1.0
```
## 配置数据库连接字符串

FeatherSQL入门和传统的传统的Ado.Net一样只需要配置数据库的链接字符串即可。

在web.config或app.config中添加以下配置：

```
  <connectionStrings>
    <add name="Maindb" connectionString="你的数据库连接字符串"/>
  </connectionStrings>
```
默认的数据库连接字符串的name为**Maindb**不区分大小写。
## 配置数据实体

```c#
    [Table("UserInfo")]
    public class UserInfo 
    {
        /// <summary>
        /// int自增长主键
        /// </summary>
        [Column("Id", ColumnType.PrimaryKey)]
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord { get; set; }

        /// <summary>
        /// 角色Id
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; }
    }
```

FeatherSQL并不能自动生成实体，但是可以借助一些代码生成器来生成实体。上面的实体中有一些特性需要注意：

1. 需要设置**Table**特性：**它指定了当前实体对应的数据库的表名称。**
2. **ColumnType**：指定数据列的类型，有以下三种：
    1. **None**：默认的不需要额外设置；
    2. **ReadOnly**：只读的冗余属性，新增和修改的时候不操作此列；
    3. **PrimaryKey**：int类型的主键自增。
## 配置数据访问层

为了更好的维护代码，FeatherSQL依然推荐大家使用分层的框架，推荐数据访问层如下：

```
    /// <summary>
    /// 数据访问层
    /// </summary>
    public class UserInfoDAO:BaseDAO<UserInfo>
    {
        /// <summary>
        /// 可以指定连接数据库的构造函数
        /// </summary>
        /// <param name="conName">连接字符串的name</param>
        public UserInfoDAO(string conName="maindb"):base(conName)
        {

        }
    }
```
在数据访问层中我们只需要继承BaseDAO并指定数据实体即可，如果需要一个数据实体连接多个数据库，则需要配置上面示例中的构造函数。
# 常用方法

## Insert、InsertAsync

```
        private static void Main(string[] args)
        {
            /// <summary>
            /// 创建数据访问层对象
            /// </summary>
            UserInfoDAO userInfoDAO = new UserInfoDAO();
            
            //这里返回的不是受影响的行数，而是新增成功的Id
            int insertId = userInfoDAO.Insert(new UserInfo
            {
                UserName = "测试新增",
                RoleId = 1,
                Phone = "185****9782",
                PassWord = "666666"
            });
        }
```
执行的SQL语句：
```
insert into [UserInfo] (UserName,PassWord,RoleId,Phone) values (@UserName,@PassWord,@RoleId,@Phone ) select SCOPE_IDENTITY()
```
和Ado.Net不太一样的是，新增单个实体，**返回的是新增成功的id**，我们依然可以用返回值是否大于0来判断是否执行成功了，如果我们需要拿到id做后续业务处理的时候就不用再次查询了。
## InsertList、InsertListAsync

```
/// <summary>
/// 创建数据访问层对象
/// </summary>
UserInfoDAO userInfoDAO = new UserInfoDAO();

List<UserInfo> insertUserInfoList = new List<UserInfo>();
for (int i = 0; i < 3; i++)
{
    insertUserInfoList.Add(new UserInfo
    {
      UserName = "批量新增"+i,
      RoleId = 1,
      Phone = "185****9782",
      PassWord = "666666"
    });
}
//这里返回的是受影响的行数，此方法一次执行，失败则回滚
int count= userInfoDAO.InsertList(insertUserInfoList);
```
执行的SQL：
```
insert into [UserInfo] (UserName,PassWord,RoleId,Phone)values(@_0UserName,@_0PassWord,@_0RoleId,@_0Phone);
insert into [UserInfo] (UserName,PassWord,RoleId,Phone)values(@_1UserName,@_1PassWord,@_1RoleId,@_1Phone); 
insert into [UserInfo] (UserName,PassWord,RoleId,Phone)values(@_2UserName,@_2PassWord,@_2RoleId,@_2Phone) 
```
## Update、UpdateAsync

### 一般方法

修改操作我参照微软EF的方式，推荐先查询后修改。

```
/// <summary>
/// 创建数据访问层对象
/// </summary>
UserInfoDAO userInfoDAO = new UserInfoDAO();

var entity = userInfoDAO.Get(66);
entity.PassWord = "888888";
entity.RoleId = 2;
entity.UserName = "测试修改";
entity.Phone = "1300000000";
int updateCount= userInfoDAO.Update(entity);
```
如已经查询过了实体可以使用如下方式：
```
int updateCount= userInfoDAO.Update(new UserInfo
{
    UserName = "测试修改",
    RoleId = 1,
    Phone = "185****9782",
    PassWord = "666666",
    Id=66//注意这里必须指定要修改的id
});
```
执行的SQL为：
```
update [UserInfo] set  UserName=@UserName,PassWord=@PassWord,RoleId=@RoleId,Phone=@Phone where Id=@Id
```
### 泛型方法

上述的方法只能根据id去修改某一条数据，如果想修改指定条件的数据，可以使用以下方法：

```
var entity = userInfoDAO.Get(66);
entity.PassWord = "888888";
entity.RoleId = 2;
entity.UserName = "测试修改";
entity.Phone = "1300000000";
userInfoDAO.Update<UserInfo>(entity, x => x.RoleId == 2);
```
执行的SQL为：
```
update [UserInfo] set UserName=@UserName,PassWord=@PassWord,RoleId=@RoleId,Phone=@Phone where (RoleId = @RoleId)
```

如果需要修改指定的某一列，比如修改密码，或者修改用户名，使用上面的方法就有些麻烦。FeatherSQL推荐使用DTO来作为查询对象，如常见的修改密码输入对象如下：

```
    /// <summary>
    /// 用于修改密码的输入对象
    /// </summary>
    [Table("UserInfo")]    
    public class UpdatePassWordInput
    {
        /// <summary>
        /// int自增长主键
        /// </summary>
        [Column("Id", ColumnType.PrimaryKey)]
        public int Id { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord { get; set; }
    }
```
如修改id为66的密码为123456则可以使用如下方法，使得业务代码更为简洁。
```
 UpdatePassWordInput input = new UpdatePassWordInput 
  {
    Id=66,
    PassWord="123456"
  };
int updateTCount =userInfoDAO.Update<UpdatePassWordInput>(input, x => x.Id == input.Id);
```
执行的SQL为：
```
update [UserInfo] set PassWord=@PassWord where (Id = @Id)
```

## UpdateList、UpdateListAsync

```
 List<UserInfo> updateUserInfoList = new List<UserInfo>();
            for (int i = 0; i < 3; i++)
            {
                updateUserInfoList.Add(new UserInfo
                {
                    UserName = "批量修改" + i,
                    RoleId = 1,
                    Phone = "185****9782",
                    PassWord = "666666",
                    Id=i//注意这里必须指定id
                });
            }
            int updateListCount = userInfoDAO.UpdateList(updateUserInfoList);
```
执行的SQL为：
```
update [UserInfo] set UserName=@_0UserName,PassWord=@_0PassWord,RoleId=@_0RoleId,Phone=@_0Phone where Id=@_0Id;
update [UserInfo] set UserName=@_1UserName,PassWord=@_1PassWord,RoleId=@_1RoleId,Phone=@_1Phone where Id=@_1Id;
update [UserInfo] set UserName=@_2UserName,PassWord=@_2PassWord,RoleId=@_2RoleId,Phone=@_2Phone where Id=@_2Id;
```

## Delete、DeleteAsync

注意：1.1.1之前的版本不支持软删除。

基于常用的后台信息管理系统，FeatherSQL提供了多种删除的重载：

1. 最常用的根据id删除
```
//返回受影响的行数
int count= userInfoDAO.Delete(123);
```
执行的SQL语句为：
```
//软删除
Update [UserInfo] SET IsDeleted=1,DeleteTime=GETDATE() where Id=@Id
//物理删除
Delete [UserInfo] where Id=@Id
```


