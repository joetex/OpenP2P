/* --------------------------------------------------------------------------
 * Copyrights
 *
 * Portions created by or assigned to Sébastien Gissinger
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
    /// RFC 4013, SASLprep - Stringprep Profile for User Names and Passwords http://tools.ietf.org/html/rfc4013
    /// </summary>
    public class SASLprep : Profile
    {
        /// <summary>
        /// Create a SASLprep instance
        /// </summary>
        public SASLprep()
            : base(new ProfileStep[] { B_1, NFKC,
                                       C_1_2, C_2_1, C_2_2, C_3, C_4, C_5, C_6, C_7, C_8, C_9,
                                       BIDI, UNASSIGNED })
        { }
    }
}