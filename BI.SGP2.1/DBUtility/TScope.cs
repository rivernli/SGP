using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;

namespace SGP.DBUtility
{
    /// <summary>
    /// TScope 的摘要说明。
    /// </summary>
    public class TScope : IDisposable
    {
        private IDbTransaction trans = null;
        private ArrayList errors;
        private bool throwOnError = true;
        private bool disposed = false;

        private const string DATA_KEY = "TRANSACTION_OBJECT";

        private bool committed = false;
        private bool rollback = false;

        public TScope() : this(true)
        {

        }

        public TScope(bool throwOnError)
        {
            this.throwOnError = throwOnError;

            Initialize();
        }

        /// <summary>
        /// 获取当前上下文的 TScope 对象
        /// </summary>
        public static TScope Current
        {
            get
            {
                Stack stack = (Stack)CallContext.GetData(DATA_KEY);
                if (stack == null || stack.Count == 0)
                {
                    return null;
                }

                return (TScope)stack.Peek();
            }
        }
        
        public static void HandlError(Exception error)
        {
            TScope ts = TScope.Current;
            if (ts != null)
            {
                ts.AddError(error);
                if (ts.throwOnError)
                {
                    throw error;
                }
            }
            else
            {
                throw error;
            }
        }

        public static bool ContextHasErrors
        {
            get
            {
                TScope ts = TScope.Current;
                if (ts == null)
                    return false;

                return ts.HasErrors;
            }
        }


        /// <summary>
        /// 获取一个布值，此值指示是否有错误
        /// </summary>
        public bool HasErrors
        {
            get
            {
                return (errors != null && errors.Count != 0);
            }
        }

        /// <summary>
        /// 获取错误信息
        /// </summary>
        public ArrayList Errors
        {
            get
            {
                if (errors == null)
                {
                    errors = new ArrayList();
                }

                return errors;
            }
        }

        public bool ThrowOnError
        {
            get
            {
                return throwOnError;
            }
            set
            {
                throwOnError = value;
            }
        }

        public IDbTransaction Transaction
        {
            get
            {
                return this.trans;
            }
        }

        private void Initialize()
        {
            Stack stack = (Stack)CallContext.GetData(DATA_KEY);
            if (stack == null)
            {
                stack = new Stack();
                CallContext.SetData(DATA_KEY, stack);
                trans = BeginTrans();
            }
            else
            {
                trans = Current.trans;
            }

            stack.Push(this);
        }

        public SqlTransaction BeginTrans()
        {
            SqlConnection cn = new SqlConnection(DBUtility.DBConnectionString);
            cn.Open();

            return cn.BeginTransaction();
        }

        public void AddError(Exception err)
        {
            Errors.Add(err);
        }


        public void Commit()
        {
            if (committed) // 已经提交
                return;

            IDbTransaction tr = this.Transaction;
            if (tr != null && tr.Connection != null
                && tr.Connection.State != ConnectionState.Closed)
            {
                try
                {
                    tr.Commit();
                }
                catch/*(Exception err)*/
                {
                    // TODO:
                }
                committed = true;
            }
        }


        public void Rollback()
        {
            if (rollback) // 已经回滚
                return;

            IDbTransaction tr = this.Transaction;
            if (tr != null && tr.Connection != null
                && tr.Connection.State != ConnectionState.Closed)
            {
                try
                {
                    tr.Rollback();
                }
                catch/*(Exception error)*/
                {
                    // TODO:
                }

                rollback = true;
            }
        }

        private void CommitOrRollback()
        {
            if (!this.HasErrors)
            {
                this.Commit();
            }
            else
            {
                this.Rollback();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Stack stack = (Stack)CallContext.GetData(DATA_KEY);
                    if (stack == null || stack.Count == 1)
                    {
                        try
                        {
                            CommitOrRollback();
                            IDbTransaction tr = this.Transaction;
                            if (tr != null && tr.Connection != null)
                            {
                                tr.Connection.Close();
                            }

                            tr.Dispose();
                        }
                        catch/*(Exception err)*/
                        {
                            // TODO:
                        }
                    }

                    if (stack != null && stack.Count != 0)
                    {
                        stack.Pop();
                        if (stack.Count == 0)
                        {
                            CallContext.SetData(DATA_KEY, null);
                        }
                    }
                }

                disposed = true;
            }
        }
    }
}

