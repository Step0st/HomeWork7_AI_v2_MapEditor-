using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;

namespace Search
{
    public class AStarBotPlayer : PlayerInput
    {
        [SerializeField] private ZombieMap _zombieMap;
        [SerializeField] private LevelMap _levelMap;
        [SerializeField] private Transform _player;
        [SerializeField] private GameObject _base;
        [SerializeField] private float _fireDistance;

        private int[,] _terrainMap;
        private int _deltaX;
        private int _deltaZ;
        private int _closestTarget;

        public override (Vector3 moveDirection, Quaternion viewDirection, bool shoot) CurrentInput()
        {
            
            var alivePositions = _zombieMap.AlivePositions();
            var alivePaths = _zombieMap.AlivePaths();

            if (alivePaths.Count != 0)
            {
               int minVal = alivePaths.Min();
               _closestTarget = alivePaths.IndexOf(minVal);
            }
            
            Vector3 targetPosition = alivePositions.Count != 0 ? alivePositions[_closestTarget] : _base.transform.position;
            var playerPosition = _player.position;
            
            var from = ToInt(playerPosition);
            var to = ToInt(targetPosition);
            
            var path = AStarFromGoogle.FindPath(_terrainMap, @from, to);
            var nextPathPoint = path.Count >= 2 ? path[1] : to;
            nextPathPoint = new Vector2Int(nextPathPoint.x - _deltaX, nextPathPoint.y - _deltaZ);
            
            var moveDirection = new Vector3(nextPathPoint.x, playerPosition.y,nextPathPoint.y) - playerPosition;
            var directVector = targetPosition - playerPosition;
            bool shouldShoot = alivePositions.Count != 0 && (targetPosition - playerPosition).magnitude <= _fireDistance;
            
            return (moveDirection, Quaternion.LookRotation(directVector), shouldShoot);
        }

        private void Awake()
        {
            var maxX = _levelMap.Points.Max(p => Mathf.RoundToInt(p.x));
            var minX = _levelMap.Points.Min(p => Mathf.RoundToInt(p.x));
            var maxZ = _levelMap.Points.Max(p => Mathf.RoundToInt(p.z));
            var minZ = _levelMap.Points.Min(p => Mathf.RoundToInt(p.z));

            _deltaX = minX < 0 ? -minX : 0;
            _deltaZ = minZ < 0 ? -minZ : 0;
            _terrainMap = new int[maxX + _deltaX + 1, maxZ + _deltaZ + 1];
            
            foreach (var point in _levelMap.Points)
            {
                _terrainMap[_deltaX + Mathf.RoundToInt(point.x), _deltaZ + Mathf.RoundToInt(point.z)] = -1;
            }
        }

        private Vector2Int ToInt(Vector3 vector3) =>
            new Vector2Int(_deltaX + Mathf.RoundToInt(vector3.x), _deltaZ + Mathf.RoundToInt(vector3.z));
    }
}