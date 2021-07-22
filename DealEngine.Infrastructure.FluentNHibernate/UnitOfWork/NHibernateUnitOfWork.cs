using NHibernate;
using System;
using System.Threading.Tasks;


namespace DealEngine.Infrastructure.FluentNHibernate
{
    public class NHibernateUnitOfWork : IUnitOfWork
    {
        private ISession _session;
        private ITransaction _transaction;
        private ISessionFactory _sessionFactory;

        public NHibernateUnitOfWork(ISession session, ISessionFactory sessionFactory)
        {
            if (session == null) throw new ArgumentNullException("session");
            _session = session;
            _sessionFactory = sessionFactory;
            if (!_session.IsOpen)
                _session = _sessionFactory.OpenSession();
            _transaction = _session.BeginTransaction();

        }

        public async Task Commit()
        {
            if (!_session.IsOpen)
                _session = _sessionFactory.OpenSession();
            _transaction = _session.BeginTransaction();

            try
            {               
              _transaction.Commit();
            }
            catch(Exception ex)
            {
                // log exception here
                await Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                CloseTransaction();
                // CloseSession();
            }

        }

        public async Task Rollback()
        {
            await _transaction.RollbackAsync();
        }

        public void CloseTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        //public void CloseSession()
        //{
        //    if (_session != null)
        //    {
        //        _session.Close();
        //    }

        //}


        public void Dispose() { }

        public IUnitOfWork BeginUnitOfWork()
        {
            return new NHibernateUnitOfWork(_session, _sessionFactory);
        }
    }
}
