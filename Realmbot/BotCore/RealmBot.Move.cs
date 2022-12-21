using System;
using System.Collections.Generic;
using BotCore.Structures;

namespace BotCore {
    public partial class RealmBot {
        public MoveAction CurrentMoveAction {
            get {
                lock (_moveActionsSync) {
                    return _moveActions.Count > 0 ? _moveActions[0] : null;
                }
            }
        }

        private readonly object _moveActionsSync = new object();
        private readonly List<MoveAction> _moveActions;

        /// <summary>
        /// Clears all move actions the bot has.
        /// </summary>
        public void ClearMoveActions() {
            lock (_moveActionsSync) {
                _moveActions.Clear();
            }
        }

        /// <summary>
        /// Adds a move action for the bot.
        /// </summary>
        /// <param name="target">Position for <see cref="RealmBot"/> to walk to.</param>
        /// <param name="callback">Callback action when <see cref="RealmBot"/> has reached the target Position;.</param>
        public void AddMoveAction(WorldPosData target, Action<RealmBot> callback = null) {
            lock (_moveActionsSync) {
                MoveAction moveAction = null;
                moveAction = new MoveAction(target, bot => {
                    bot.RemoveMoveAction(moveAction);
                    callback?.Invoke(this);
                });
                _moveActions.Add(moveAction);
            }
        }

        public void AddMoveAction(double x, double y, Action<RealmBot> callback = null) {
            lock (_moveActionsSync) {
                MoveAction moveAction = null;
                moveAction = new MoveAction(new WorldPosData(x, y), bot => {
                    bot.RemoveMoveAction(moveAction);
                    callback?.Invoke(this);
                });
                _moveActions.Add(moveAction);
            }
        }

        /// <summary>
        /// Removes a specific move action from <see cref="_moveActions"/>
        /// </summary>
        /// <param name="moveAction">The move action that is going to be removed from <see cref="_moveActions"/></param>
        public void RemoveMoveAction(MoveAction moveAction) {
            lock (_moveActionsSync) {
                _moveActions.Remove(moveAction);
            }
        }

        /// <summary>
        /// Gets the next location for <see cref="RealmBot"/> to move towards.
        /// </summary>
        /// <param name="finished">Depicts whether the current move action is completed.</param>
        /// <returns>Next location for <see cref="RealmBot"/></returns>
        public WorldPosData GetNextLocation(out bool finished) {
            WorldPosData pos = WorldPosData.Zero;
            finished = false;

            if (CurrentMoveAction == null)
                return new WorldPosData();

            int delta = Math.Min(World.LastUpdate - World.LastMoveTime, 200);
            double speed = Player.GetMoveSpeed();
            double dist = speed * delta;

            if (CurrentMoveAction.Position.Distance(Player.Position) < dist)
                pos = CurrentMoveAction.Position;

            if (pos == WorldPosData.Zero) {
                double angle = Player.Position.GetAngle(CurrentMoveAction.Position);
                float newX = (float) (Player.X + dist * Math.Cos(angle));
                float newY = (float) (Player.Y + dist * Math.Sin(angle));

                pos = new WorldPosData {X = newX, Y = newY};
            }

            if (CurrentMoveAction.Position == pos) finished = true;

            return pos;
        }
    }
}