/********************************************************************************
** auth： qinjiancai
** date： 2022/5/24 16:02:00
** desc： tab文件读写，参照c++库
** Ver.:  V1.0.0
*********************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;


public class TableFile
{
    Stream m_tableStream;
    StreamReader m_tableReader;
	Encoding m_fileEncoding;

    Dictionary<string, int> m_attrDict = null;  /*表格属性列 */
    int m_RowCount = 0;
    int m_CursorPos = 0;
    string[] m_CachedColumns; // 当前行缓存(已拆分)


    /** 构造器 */
    private TableFile() { }

    /*  从文件对象中创建 */
    public static TableFile LoadFromFile(string path, bool isEditor = false)
    {
		TableFile tableFile = null;
		path = path.Trim();
		if (path.Length > 0)
		{
            byte[] fileContent = FilesManager.Instance.GetFileContent(path, "bytes", isEditor);

            if (fileContent != null)
                tableFile = TableFile.LoadFromContent(fileContent);
		}
			
        return tableFile;
    }

    /* 从字符串中创建对象 */
    public static TableFile LoadFromContent(byte[] data)
    {
        TableFile tableFile = new TableFile();
			
		tableFile.m_fileEncoding = DetectedFileEncoding(data);
        tableFile.m_tableStream = new MemoryStream(data);

        tableFile.ParseColumnNames(data);
        tableFile.ParseRowCount(data);
        tableFile.InitStreamReader(); // reset cursor position

        return tableFile;
    }
		 
	static Encoding DetectedFileEncoding(byte[] data)
	{
		var result = GBK.GBKEncodingManager.Encoding;
			
		if (data.Length > 3 && data[0] >= 0xEF)
		{            
			if (data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
			{
	            result = System.Text.Encoding.UTF8;   
	        }
	        else if (data[0] == 0xFE && data[1] == 0xFF && data[2] == 0x00)
	        {
                result = System.Text.Encoding.BigEndianUnicode;   
	        }
	        else if (data[0] == 0xFF && data[1] == 0xFE && data[2] == 0x41)
	        {
                result = System.Text.Encoding.Unicode;
	        }
	    }

        if(IsUTF8Bytes(data))
            result = System.Text.Encoding.UTF8;

        return result;
	}

    private static bool IsUTF8Bytes(byte[] data)
    {
        int charByteCounter = 1;
        //计算当前正分析的字符应还有的字节数
        byte curByte; //当前分析的字节.
        for (int i = 0; i < data.Length; i++)
        {
            curByte = data[i];
            if (charByteCounter == 1)
            {
                if (curByte >= 0x80)
                {
                    //判断当前
                    while (((curByte <<= 1) & 0x80) != 0)
                    {
                        charByteCounter++;
                    }
                    //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X
                    if (charByteCounter == 1 || charByteCounter > 6)
                    {
                        return false;
                    }
                }
            }
            else
            {
                //若是UTF-8 此时第一位必须为1
                if ((curByte & 0xC0) != 0x80)
                {
                    return false;
                }
                charByteCounter--;
            }
        }
        if (charByteCounter > 1)
        {
            throw new Exception("非预期的byte格式");
        }
        return true;
    }

    /* 初始化实例对象的Reader，回到起点，用于下一次读取 */
    public void InitStreamReader()
    {
        this.m_tableStream.Seek(0, 0);
        m_CursorPos = 0;

        this.m_tableReader = new StreamReader(this.m_tableStream, this.m_fileEncoding);
    }

    // 开始实例方法

    /// 初始化列名表
    private bool ParseColumnNames(byte[] bytes)
    {
        var streamReader = new StreamReader(new MemoryStream(bytes), this.m_fileEncoding);
        m_attrDict = new Dictionary<string, int>();

        string attrLine = streamReader.ReadLine();
        attrLine = attrLine.Trim();
		attrLine = attrLine.TrimEnd('\t',' ');
        string[] attrArray = attrLine.Split('\t');

        for (int i = 0; i < attrArray.Length; i++)
        {
            if (string.IsNullOrEmpty(attrArray[i])) continue;
            /* 列从1开始，  i+1所以 */
			try
			{
                this.m_attrDict.Add(attrArray[i], i + 1);  /* 放入字典 -> ( ColumnName,  ColumnNum ) (名字，列数）*/
			}
			catch(Exception e)
			{
                Debug.LogError("添加列名" + attrArray[i] + " 时出错: " + e.Message);
				continue;	
			}
        }
        return true;
    }

    public string GetRowName(int index)
    {
        foreach (var dict in m_attrDict)
        {
            if (dict.Value == index) return dict.Key;
        }
        return null;
    }

    /* 根据列名获取列的数字 */
    private int findColumnByName(string columnName)
    {
		if (m_attrDict.ContainsKey(columnName))
		{
            return this.m_attrDict[columnName];
		}
			
		Debug.LogError(string.Format("找不到列名[{0}]!在表", columnName));
		return 0;
    }

    /** 获取第一行第column列的名字， 即表头属性名 */
    public string GetColumnName(int column)
    {
        foreach (KeyValuePair<string, int> attr in this.m_attrDict)
        {
            if (attr.Value == column)
            {
                return attr.Key;
            }
        }

        /* 没找到 */
        return null;
    }


    /** 列数 */
    public int GetColumnsCount()
    {
        return m_attrDict.Count;
    }

    /* 设置值, 返回成功与否， 不抛出异常 */
    public bool GetString(int row, string columnName, string defaultVal, ref string outVal)
    {
        try
        {
            int col = findColumnByName(columnName);
            outVal = GetString(row, col);
        }
        catch (IndexOutOfRangeException)
        {
            outVal = defaultVal;  // 取默认值
        }

        return true;
    }

    /* 获取表格内容字符串 */
    public string GetString(int row, int column)
    {
        if (row < (m_CursorPos - 1))
            this.InitStreamReader(); // 一般不会往前读，避免了seek

        if (row > m_RowCount)
            throw new IndexOutOfRangeException();

        if (row == (m_CursorPos - 1))
        {
            return m_CachedColumns[column - 1].Trim(); // 去空格
        }

        do
        {
            string line = m_tableReader.ReadLine();
            if (line == null)
                break;
            if (line == string.Empty)
                continue; // 注意没有递增m_CursorPos

            m_CursorPos++; // 递增行数游标

            if (row == (m_CursorPos - 1))
            {
                m_CachedColumns = line.Split('\t');

                return m_CachedColumns[column - 1].Trim();  // 去空格
            }
        } while (true);

        // 仍然没找到？内部状态乱了
        throw new IndexOutOfRangeException();
    }

    /** Float */
    public float GetFloat(int row, int column)
    {
        return Convert.ToSingle(this.GetString(row, column));
    }
    public float GetFloat(int row, string columnName)
    {
        return this.GetFloat(row, this.findColumnByName(columnName));
    }

    public bool GetFloat(int row, string columnName, float defaultVal, ref float outVal)
    {
        try
        {
            outVal = this.GetFloat(row, this.findColumnByName(columnName));
            return true;
        }
        catch (IndexOutOfRangeException)
        {
            outVal = defaultVal;
            return false;
        }
        catch (FormatException)
        {
            // FormatException -> 值为""， 空字符串
            outVal = defaultVal;
            return false;
        }
    }

    /**Integer*/
    public int GetInteger(int row, int column)
    {
        return Convert.ToInt32(this.GetString(row, column));
    }

    public int GetInteger(int row, string columnName)
    {
        return this.GetInteger(row, this.findColumnByName(columnName));
    }

    public bool GetInteger(int row, string columnName, int defaultVal, ref int outVal)
    {
        try
        {
            outVal = this.GetInteger(row, this.findColumnByName(columnName));

            return true;
        }
        catch (IndexOutOfRangeException)
        {
            outVal = defaultVal;
            return false;
        }
        catch (FormatException)
        {
            // FormatException -> 值为""， 空字符串
            outVal = defaultVal;
            return false;
        }
    }

    /* UInt 32 */
    public uint GetUInteger(int row, int column)
    {
        return Convert.ToUInt32(this.GetString(row, column));
    }
		
	public byte GetByte(int row, int column)
	{
		return Convert.ToByte(this.GetString(row, column));
	}

    public uint GetUInteger(int row, string columnName)
    {
        return this.GetUInteger(row, this.findColumnByName(columnName));
    }

    public bool GetUInteger(int row, string columnName, uint defaultVal, ref uint outVal)
    {
        try
        {
            outVal = this.GetUInteger(row, this.findColumnByName(columnName));

            return true;
        }
        catch (IndexOutOfRangeException)
        {
            outVal = defaultVal;
            return false;
        }
        catch (FormatException)
        {
            // FormatException -> 值为""， 空字符串
            outVal = defaultVal;
            return false;
        }
    }
		
	public bool GetByte(int row, string columnName, byte defaultVal, ref byte outVal)
	{
		try
        {
            outVal = this.GetByte(row, this.findColumnByName(columnName));

            return true;
        }
        catch (IndexOutOfRangeException)
        {
            outVal = defaultVal;
            return false;
        }
        catch (FormatException)
        {
            // FormatException -> 值为""， 空字符串
            outVal = defaultVal;
            return false;
        }
	}

    private bool ParseRowCount(byte[] bytes)
    {
		StreamReader sr = new StreamReader(new MemoryStream(bytes), this.m_fileEncoding);
		m_RowCount = 0;
			
		sr.ReadLine(); // 挑过第一行
		while (sr.ReadLine() != null)
		{
			++m_RowCount;
		}
			
		return true;
    }

    /// 获取行数（不含第一行的ColumnHeader）
    public int GetRowsCount()
    {
        return m_RowCount;
    }

    public int GetCount()
    {
        return m_attrDict.Count;
    }

    public void Close()
    {
        m_tableReader.Close();
        m_tableStream = null;
        m_tableReader = null;
    }
}
