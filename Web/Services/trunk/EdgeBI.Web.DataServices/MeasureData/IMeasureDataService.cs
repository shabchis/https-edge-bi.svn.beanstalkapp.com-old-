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
		List<ReturnData> GetData
		(
			int accountID,
			DataGrouping grouping,
			int top,
			MeasureRef[] measures,
			DayCodeRange[] ranges,
			DiffType diff,
			MeasureSort[] dataSort,
			MeasureSort[] viewSort,
			bool format
		);

		/* EXAMPLE:
		 * http://../data?accountID=95&
		 *		grouping=campaign&
		 *		top=10&
		 *		ranges=20100101-20100131(main),20091201-20091231&
		 *		measures=17(9,2,3),T9,3,18(3)&
		 *		diff=both&
		 *		datasort=m1(absolute,desc),m2(relative,asc)&
		 *		viewsort=m3(asc),m4(desc)
		 *		format=true
		 
		 
		 ID		Name		M1.Range1.Value		M1.Range1.DiffAbs		M1.Range1.DiffRel		M1.Range2.Value
		 ----------------------------------------------------------------------------------------------------------
		 1		Gold		12312				135						0.03123					12312
		 2		Silver		4323				2341					0.48123					1998
		 
		 */

	}

}
