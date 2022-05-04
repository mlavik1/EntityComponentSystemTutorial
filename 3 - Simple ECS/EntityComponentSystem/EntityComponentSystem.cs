/// <summary>
/// Interface for all components.
/// </summary>
public interface IComponentData
{

}

/// <summary>
/// System for dealing with entities and components. Entities are basically just integers, and components are structs inheriting from IComponentData.
/// This is a very naive implementation, used as an example of how ECS should not be implemented.
/// All components are stored in their own lists, where the index is equal to the entity ID.
/// All these lists have the same size, which is equal to the maximum entity count, which could result in very high memory usage!
/// </summary>
public class EntityComponentSystem
{
    private interface ComponentArrayBase
    {
        void OnEntityRemoved(int entity);
    }

    private class ComponentArray<T> : ComponentArrayBase
     where T : struct, IComponentData
    {
        public List<T> components = null;

        public virtual void OnEntityRemoved(int entity)
        {
            components[entity] = default(T);
        }
    }

    private int maxEntities;
    private Queue<int> availableEntities = null;
    private Dictionary<Type, ComponentArrayBase> componentArrays = new Dictionary<Type, ComponentArrayBase>();

    public EntityComponentSystem(int maxEntities)
    {
        this.maxEntities = maxEntities;
        
        availableEntities = new Queue<int>(maxEntities);
        for (int i = 0; i < maxEntities; ++i)
        {
            availableEntities.Enqueue(i);
        }
    }

    public int AddEntity()
    {
        System.Diagnostics.Debug.Assert(availableEntities.Count > 0);
        return availableEntities.Dequeue();
    }

    public List<int> AddEntities(int count)
    {
        System.Diagnostics.Debug.Assert(count <= availableEntities.Count);
        List<int> entities = new List<int>(count);
        for (int i = 0; i < count; ++i)
            entities.Add(availableEntities.Dequeue());
        return entities;
    }

    public void RemoveEntity(int entity)
    {
        availableEntities.Enqueue(entity);

        foreach (var compPair in componentArrays)
            compPair.Value.OnEntityRemoved(entity);
    }

    public void SetComponent<T>(int entity, T comp) where T : struct, IComponentData
    {
        ComponentArray<T> compArr = GetComponentArray<T>();
        compArr.components[entity] = comp;
    }

    public T GetComponent<T>(int entity) where T : struct, IComponentData
    {
        ComponentArray<T> compArr = GetComponentArray<T>();
        return compArr.components[entity];
    }

    private ComponentArray<T> GetComponentArray<T>()  where T : struct, IComponentData
    {
        Type compType = typeof(T);
        ComponentArray<T> compArr;
        if (!componentArrays.ContainsKey(compType))
        {
            compArr = new ComponentArray<T>();
            compArr.components = Enumerable.Repeat(default(T), maxEntities).ToList();
            componentArrays.Add(compType, compArr);
        }
        else
        {
            compArr = (ComponentArray<T>)componentArrays[compType];
        }
        return compArr;
    }
}
