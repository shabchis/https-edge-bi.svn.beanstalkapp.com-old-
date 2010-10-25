using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Easynet.Edge.Core.Utilities;

namespace EdgeBI.Web.DataServices
{
	[ServiceContract]
	public interface IMeasureDataService
	{
       [OperationContract]
		List<Measure> GetMeasures(int accountID);

        [OperationContract]
        List<Channel> GetChannels();

		[OperationContract]
		List<ObjData> GetData
		(
			int account,
			DataGrouping grouping,
			int top,
			MeasureRef[] measures,
			DayCodeRange[] ranges,
			MeasureDiff[] diff,
			MeasureSort[] dataSort,
			MeasureSort[] viewSort,
			MeasureFormat[] format,
            Mode mode
		);

		/* EXAMPLE:
		 * http://../data?account=95&
		 *		grouping=campaign&
		 *		top=10&
		 *		ranges=20100101-20100131,20091201-20091231&
		 *		measures=17(9,2,3),T9,3,18(3)&
		 *		diff=m1(both),m2(diffrel),m3(none),m4(none)
		 *		filter=m1.range1.diffabs > 0 and m2.range2.value is not null
		 *		datasort=m1.range1.diffabs(desc),m2.range1.diffrel(asc)&
		 *		viewsort=m3(asc),m4(desc)
		 *		format=m1(true,both),m2(false,diffabs)
		 
		 
		 ID		Name		M1.Range1.Value		M1.Range1.DiffAbs		M1.Range1.DiffRel		M1.Range2.Value
		 ----------------------------------------------------------------------------------------------------------
		 1		Gold		12312				135						0.03123					12312
		 2		Silver		4323				2341					0.48123					1998
		 
		 */

	}

}
