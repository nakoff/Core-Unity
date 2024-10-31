using System.Collections.Generic;

namespace Core.LLEcs
{
    public interface IEcsSystem
    {
        public EcsFilter Filter { get; }
    }

	public interface IEntityAdded : IEcsSystem
	{
		void EntitiesAdded(List<EcsEntity> entities);
	}

	public interface IEntitiesUpdate : IEcsSystem
	{
		void EntitiesUpdate(Dictionary<int, EcsEntity> entities, float deltaTime);
	}

	public interface IEntitiesFixedUpdate : IEcsSystem
	{
		void EntitiesFixedUpdate(Dictionary<int, EcsEntity> entities, float deltaTime);
	}

	public interface IEntityRemoved : IEcsSystem
	{
		void EntitiesRemoved(List<int> ids);
	}

	public abstract class EcsSystem
	{
		public int Id { get; }
		public abstract EcsFilter Filter { get; }

		protected EcsWorld world;

		public EcsSystem(EcsWorld world)
		{
			this.world = world;
			Id = GetHashCode();
		}
	}
}
