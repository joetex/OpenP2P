/* --------------------------------------------------------------------------
 * Copyrights
 *
 * Portions created by or assigned to Cursive Systems, Inc. are
 * Copyright (c) 2002-2008 Cursive Systems, Inc.  All Rights Reserved.  Contact
 * information for Cursive Systems, Inc. is available at
 * http://www.cursive.net/.
 *
 * License
 *
 * Jabber-Net is licensed under the LGPL.
 * See LICENSE.txt for details.
 * --------------------------------------------------------------------------*/
using StringPrep.Steps;

namespace StringPrep
{
    /// <summary>
    /// RFC 3491, "nameprep" profile, for internationalized domain names.
    /// </summary>
    public class Nameprep : Profile
    {
        /// <summary>
        /// Create a nameprep instance.
        /// </summary>
        public Nameprep()
            : base(new ProfileStep[] { B_1, B_2, NFKC,
                                       C_1_2, C_2_2, C_3, C_4, C_5, C_6, C_7, C_8, C_9,
                                       BIDI, UNASSIGNED})
        { }
    }
}