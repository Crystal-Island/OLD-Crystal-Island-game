using System.Collections.Generic;
using System;

namespace Polymoney
{
    /// <summary>
    /// Contains serializable level data.
    /// </summary>
    [Serializable]
    public class LevelData
    {
        /// <summary>
        /// Contains the set of valid personal background info.
        /// </summary>
        public List<Person> Persons = new List<Person>();
        /// <summary>
        /// Contains the set of personal background info for the mayor.
        /// </summary>
        public List<Person> MayorPersons = new List<Person>();
        /// <summary>
        /// Contains the set of valid player homes.
        /// </summary>
        public List<Home> Homes = new List<Home>();
        /// <summary>
        /// Contains the set of homes for the mayor.
        /// </summary>
        public List<Home> MayorHomes = new List<Home>();
        /// <summary>
        /// Contains the set of jobs that signify unemployment.
        /// Can be used to provide multiple unemployment
        /// backgrounds.
        /// </summary>
        public List<Job> Unemployed = new List<Job>();
        /// <summary>
        /// Contains the set of valid player jobs.
        /// </summary>
        public List<Job> Jobs = new List<Job>();
        /// <summary>
        /// Contains the set of jobs for the mayor.
        /// </summary>
        public List<Job> MayorJobs = new List<Job>();
        /// <summary>
        /// Contains the set of valid player talents.
        /// </summary>
        public List<Talent> Talents = new List<Talent>();
        /// <summary>
        /// Contains the set of mayor talents.
        /// </summary>
        public List<Talent> MayorTalents = new List<Talent>();
        /// <summary>
        /// Contains the set of valid events.
        /// </summary>
        public List<Incident> Incidents = new List<Incident>();
    }

}
