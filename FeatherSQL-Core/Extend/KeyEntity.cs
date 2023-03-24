namespace FeatherSQL
{
    public class KeyEntity
    {
        /// <summary>
        /// int自增长主键
        /// </summary>
        [Column("Id", ColumnType.PrimaryKey)]
        public int Id { get; set; }
    }
}
