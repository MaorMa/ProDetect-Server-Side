# RRS-SERVER
<b>This project was written by Maor Maimon and Yaniv Knobel.</b>


<b>Summary</b>

A diverse diet plays a fundamental role in a person’s health and is necessary to provide the body with the right nutritional elements. To maintain beneficial consumption habits, one must methodically follow and monitor those habits. This process could be very costly and time-consuming. However, such a task could be completed efficiently through the use of automated systems.

The system ‘ProDetect’ was created to address a need of researchers in the nutritional science field whose work is to study various families’ food purchase patterns. As a part of the research, the researchers must evaluate a family’s purchase and consumption reports by using the post-purchase receipts from any of Israel’s chain supermarkets. To do so, they must first collect the receipts from the families and manually extract information about the product, such as product names, quantities, and prices. The idea behind our system is to create an easier and more efficient research process.

The system, based on a client-server architecture, is a convenient platform for the researchers throughout the entire process. The system provides families with the ability to upload pictures any time through an accessible interface. Afterward, the system conducts several preprocessing operations to maximize the recognition result until finally, the photos are sent to the OCR engine (Optical Character Recognition). Using OCR technology along with our database (data of products in various chain supermarkets around Israel), the system can recognize products automatically. The researcher can easily view the results and adjust them if necessary. Moreover, the system can retrieve nutritional data on each product. Additionally, the system allows researchers and families to explore a variety of statistics concerning their food purchase patterns. 

Finally, after completing and publishing the system, we conducted an experiment which included nine participants who were instructed to use the system for a whole month. Post-experiment analysis shows that most participants felt that the system was intuitive and easy to use. Most of the participants said that the system helped them in one way or another to better understand their consumption and purchase habits. Additionally, in order to assess the recognition precision, we collected data from 39 receipts which showed a recall of 86.77%.

<b>Technologies</b>

Front-end: Angular, Back-end: ASP.NET WEB API, Multi-Threading, MSSQL, JWT, OCR.

<b>Pipeline</b>
![Screenshot](https://i.ibb.co/F6y3GyD/pipeline.jpg)

<b>Results</b>
![Screenshot](https://i.ibb.co/gvmFMVC/res.jpg)
