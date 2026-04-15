using System.Collections.Generic;

namespace MobilePhoneServiceAndSalesSystem.Models.Repositories
{
    public interface IEntityMockRepository<T>
    {
        IReadOnlyList<T> Items { get; }
        void Add(T item);
    }

    public class EntityMockRepository<T> : IEntityMockRepository<T>
    {
        private readonly List<T> _items = new List<T>();

        public IReadOnlyList<T> Items => _items;

        public void Add(T item)
        {
            _items.Add(item);
        }
    }

    public interface ICustomerMockRepository : IEntityMockRepository<Customer>
    {
    }

    public class CustomerMockRepository : EntityMockRepository<Customer>, ICustomerMockRepository
    {
    }

    public interface IPhoneMockRepository : IEntityMockRepository<Phone>
    {
    }

    public class PhoneMockRepository : EntityMockRepository<Phone>, IPhoneMockRepository
    {
    }

    public interface IOrderMockRepository : IEntityMockRepository<Order>
    {
    }

    public class OrderMockRepository : EntityMockRepository<Order>, IOrderMockRepository
    {
    }

    public interface IProductMockRepository : IEntityMockRepository<Product>
    {
    }

    public class ProductMockRepository : EntityMockRepository<Product>, IProductMockRepository
    {
    }

    public interface IRepairJobMockRepository : IEntityMockRepository<RepairJob>
    {
    }

    public class RepairJobMockRepository : EntityMockRepository<RepairJob>, IRepairJobMockRepository
    {
    }

    public interface ISparePartMockRepository : IEntityMockRepository<SparePart>
    {
    }

    public class SparePartMockRepository : EntityMockRepository<SparePart>, ISparePartMockRepository
    {
    }

    public interface ITechnicianMockRepository : IEntityMockRepository<Technician>
    {
    }

    public class TechnicianMockRepository : EntityMockRepository<Technician>, ITechnicianMockRepository
    {
    }
}