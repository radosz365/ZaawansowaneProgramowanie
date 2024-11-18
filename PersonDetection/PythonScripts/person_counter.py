import sys
import tensorflow as tf
import cv2
import os

def detect_people(image_path):
    model_path = os.path.join(os.getcwd(), "Models", "TensorFlowModels", "ssd_mobilenet_v2_coco_2018_03_29", "saved_model")
    model = tf.saved_model.load(model_path)

    detect_fn = model.signatures['serving_default']

    image = cv2.imread(image_path)
    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    
    input_tensor = tf.convert_to_tensor(image_rgb)
    input_tensor = input_tensor[tf.newaxis, ...]

    detections = detect_fn(input_tensor)

    detection_classes = detections['detection_classes'].numpy()[0]
    detection_scores = detections['detection_scores'].numpy()[0]

    person_count = sum(
        1 for cls, score in zip(detection_classes, detection_scores)
        if int(cls) == 1 and score > 0.5
    )

    return person_count

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python people_counter.py")
        sys.exit(1)

    image_path = sys.argv[1]
    count = detect_people(image_path)
    print(count)