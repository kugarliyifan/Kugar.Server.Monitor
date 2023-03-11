using Kugar.Server.MonitorCollectors.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Configuration;
using FreeSql;
using Kugar.Core.ExtMethod;

namespace Kugar.Server.MonitorCollectors.SQLServer
{
    public class SQLServerMonitor: UniformSubmitMonitorBase
    {
        private List<IFreeSql> _freeSql = new();
        private int _internal = 0;

        public SQLServerMonitor(IServiceProvider provider) : base(provider)
        {
            var configuration = (IConfiguration)provider.GetService(typeof(IConfiguration));

            var session=configuration.GetSection("SQLServer");

            var timer = session["Internal"].ToInt(20000);

            var connStrList = session.Get<List<string>>();

            _internal = timer;

            foreach (var item in connStrList)
            {
                _freeSql.Add(new FreeSqlBuilder().UseConnectionString(DataType.SqlServer,item).UseExitAutoDisposePool(true).Build());
            }
        }

        protected override async Task Run(IServiceProvider serviceProvider, CancellationToken stoppingToken)
        {
            await getSlowQuery();
        }

        private async Task getSlowQuery()
        {
            var sql = """
                SELECT
                    (total_elapsed_time / execution_count)/1000 N'QueryAvgElapsed'
                    ,total_elapsed_time/1000 N'总花费时间ms'
                    ,total_worker_time/1000 N'CpuTime'
                    ,total_physical_reads N'物理读取总次数'
                    ,total_logical_reads/execution_count N'每次逻辑读次数'
                    ,total_logical_reads N'逻辑读取总次数'
                    ,total_logical_writes N'逻辑写入总次数'
                    ,execution_count N'执行次数'.
                    ,SUBSTRING(st.text, (qs.statement_start_offset/2) + 1,
                    ((CASE statement_end_offset
                    WHEN -1 THEN DATALENGTH(st.text)
                    ELSE qs.statement_end_offset END
                    - qs.statement_start_offset)/2) + 1) N'Sql' 
                    ,last_execution_time N'EventDt'
                    FROM
                    sys.dm_exec_query_stats AS qs CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) as st
                    WHERE
                    SUBSTRING(st.text, (qs.statement_start_offset/2) + 1,
                    ((CASE statement_end_offset
                    WHEN -1 THEN DATALENGTH(st.text)
                    ELSE qs.statement_end_offset END
                    - qs.statement_start_offset)/2) + 1) not like '%fetch%'
                    and last_execution_time between @startDt and @endDt
                    ORDER BY 
                    total_elapsed_time / execution_count DESC;
                """;

            foreach (var freeSql in _freeSql)
            {
                var data = await freeSql.Ado.QueryAsync<SQLSlowQuery>(sql);

                await this.Submit(data);
            }


        }

        protected override int Internal => _internal;

        public override string TypeId { get; set; } = "SQLServer";
    }

    /// <summary>
    /// 慢速查询记录
    /// </summary>
    public class SQLSlowQuery : EventDataBase
    {
        public override string TypeId => "SQLServerSlowQuery";

        /// <summary>
        /// 所属数据库名称
        /// </summary>
        public string DbName { set; get; }

        /// <summary>
        /// 查询平均花费时间
        /// </summary>
        public int QueryAvgElapsed { set; get; }

        /// <summary>
        /// 所用的CPU总时间ms
        /// </summary>
        public int CpuTime { set; get; }

        /// <summary>
        /// 查询语句
        /// </summary>
        public string Sql { set; get; }
         

        /// <summary>
        /// 编译耗时
        /// </summary>
        public int CompileElapsed { set; get; }

        public override JObject Serialize()
        {
            throw new NotImplementedException();
        }

        public override void LoadFrom(string json)
        {
            throw new NotImplementedException();
        }
    }
}