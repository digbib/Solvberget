﻿using System.Collections.Generic;
using System.Linq;
using Solvberget.Domain.Documents;

namespace Solvberget.Domain.Users
{
    public class ReservationsRepository : IReservationsRepository
    {
        public IEnumerable<Reservation> GetReservations(UserInfo user)
        {
            return GetReservationsForUser(user);
        }

        public void AddReservation(Reservation res, UserInfo user)
        {
            GetReservationsForUser(user).Add(res);
        }

        public void RemoveReservation(Reservation res, UserInfo user)
        {
            var reservations = GetReservationsForUser(user);

            reservations.Remove(res);

            GetReservationsForUser(user).Remove(res);

            var flkjgd = GetReservationsForUser(user);

            var ldkjgf = "";
        }

        public bool IsReserved(Reservation res, UserInfo user)
        {
            return GetReservationsForUser(user).Contains(res);
        }

        private List<Reservation> GetReservationsForUser(UserInfo user)
        {
            return user.Reservations.ToList();
        }
    }
}
