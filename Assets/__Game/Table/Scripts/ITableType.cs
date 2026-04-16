using UnityEngine;

/// <summary>데이터 테이블 행을 Table/ID로 식별하는 인터페이스</summary>
public interface ITableType
{
    /// <summary>소속 테이블 이름</summary>
    string Table { get; }

    /// <summary>행의 고유 식별자</summary>
    string ID { get; }

    /// <summary>키로 컬럼 데이터를 조회하는 인덱서</summary>
    object this[string _id] { get; }
}
