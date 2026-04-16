using System.Collections.Generic;

/// <summary>ITableType을 구현하는 테이블 데이터의 기본 타입</summary>
public class TableType : ITableType
{
    /// <summary>소속 테이블 이름</summary>
    public string Table { get; protected set; }
    /// <summary>행의 고유 식별자</summary>
    public string ID { get; protected set; }
    /// <summary>키로 컬럼 데이터를 조회하는 인덱서</summary>
    public object this[string _id]
    {
        get
        {
            if (m_Data.TryGetValue(_id, out var data))
                return data;

            return null;
        }
    }

    protected Dictionary<string, object> m_Data = new Dictionary<string, object>();
}
